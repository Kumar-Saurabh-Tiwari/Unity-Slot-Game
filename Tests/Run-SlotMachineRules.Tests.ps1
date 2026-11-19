$ErrorActionPreference = 'Stop'

$rulesPath = Join-Path $PSScriptRoot '..\Assets\Scripts\SlotMachineRules.cs'
Add-Type -Path (Resolve-Path $rulesPath)

$script:assertionCount = 0

function Assert-Equal {
    param(
        [object]$Expected,
        [object]$Actual,
        [string]$Message
    )

    $script:assertionCount++
    if ($Expected -ne $Actual) {
        throw "$Message Expected '$Expected', received '$Actual'."
    }
}

function Assert-Throws {
    param(
        [scriptblock]$Action,
        [type]$ExceptionType,
        [string]$Message
    )

    $script:assertionCount++
    try {
        & $Action
    }
    catch {
        if ($ExceptionType.IsInstanceOfType($_.Exception)) {
            return
        }

        throw "$Message Expected $($ExceptionType.FullName), received $($_.Exception.GetType().FullName)."
    }

    throw "$Message Expected $($ExceptionType.FullName), but no exception was thrown."
}

$payoutCases = @(
    @{ Symbol = 0; Bet = 5; Multiplier = 3 },
    @{ Symbol = 1; Bet = 10; Multiplier = 2 },
    @{ Symbol = 2; Bet = 25; Multiplier = 5 },
    @{ Symbol = 3; Bet = 10; Multiplier = 10 }
)

foreach ($testCase in $payoutCases) {
    $outcome = [int[]]@($testCase.Symbol, $testCase.Symbol, $testCase.Symbol)
    Assert-Equal $true ([SlotMachineRules]::IsWinningCombination($outcome)) "Triple symbol $($testCase.Symbol) must win."
    Assert-Equal ($testCase.Bet * $testCase.Multiplier) ([SlotMachineRules]::GetWinnings($outcome, $testCase.Bet)) "Symbol $($testCase.Symbol) payout."
}

$checkedOutcomes = 0
$checkedWins = 0
for ($first = 0; $first -lt [SlotMachineRules]::SymbolCount; $first++) {
    for ($second = 0; $second -lt [SlotMachineRules]::SymbolCount; $second++) {
        for ($third = 0; $third -lt [SlotMachineRules]::SymbolCount; $third++) {
            $outcome = [int[]]@($first, $second, $third)
            $isWin = $first -eq $second -and $second -eq $third
            Assert-Equal $isWin ([SlotMachineRules]::IsWinningCombination($outcome)) "Win detection for $first-$second-$third."
            if ($isWin) {
                $checkedWins++
            }
            else {
                Assert-Equal 0 ([SlotMachineRules]::GetWinnings($outcome, 10)) "Non-winning outcome $first-$second-$third must not pay."
            }
            $checkedOutcomes++
        }
    }
}

Assert-Equal 64 $checkedOutcomes 'All possible three-reel combinations must be checked.'
Assert-Equal 4 $checkedWins 'There are four winning triples.'
Assert-Throws { [SlotMachineRules]::IsWinningCombination($null) } ([ArgumentNullException]) 'Null outcomes must be rejected.'
Assert-Throws { [SlotMachineRules]::GetWinnings([int[]]@(0, 0), 5) } ([ArgumentException]) 'Outcomes with the wrong reel count must be rejected.'
Assert-Throws { [SlotMachineRules]::GetWinnings([int[]]@(0, 1, 4), 5) } ([ArgumentOutOfRangeException]) 'Out-of-range symbols must be rejected.'
Assert-Throws { [SlotMachineRules]::GetWinnings([int[]]@(0, 0, 0), 0) } ([ArgumentOutOfRangeException]) 'A zero bet must be rejected.'
Assert-Throws { [SlotMachineRules]::CreateOutcome($null) } ([ArgumentNullException]) 'A null random generator must be rejected.'

$random = New-Object System.Random(20261119)
$symbolCounts = New-Object 'int[]' ([SlotMachineRules]::SymbolCount)
$distinctOutcomes = New-Object 'System.Collections.Generic.HashSet[string]'
$sampleCount = 40000
for ($spin = 0; $spin -lt $sampleCount; $spin++) {
    $outcome = [SlotMachineRules]::CreateOutcome($random)
    Assert-Equal [SlotMachineRules]::ReelCount $outcome.Length 'Every random outcome must contain three symbols.'
    $distinctOutcomes.Add(($outcome -join '-')) | Out-Null
    foreach ($symbol in $outcome) {
        if ($symbol -lt 0 -or $symbol -ge [SlotMachineRules]::SymbolCount) {
            throw "Random outcome contained an invalid symbol index: $symbol."
        }
        $symbolCounts[$symbol]++
    }
}

Assert-Equal $true ($distinctOutcomes.Count -gt 20) 'Random spins must not repeat a single outcome.'
$expectedCount = ($sampleCount * [SlotMachineRules]::ReelCount) / [SlotMachineRules]::SymbolCount
$chiSquare = 0.0
foreach ($count in $symbolCounts) {
    $difference = $count - $expectedCount
    $chiSquare += ($difference * $difference) / $expectedCount
}
Assert-Equal $true ($chiSquare -lt 30.0) 'Symbol outcomes must be approximately uniform in the seeded RNG sample.'

Write-Output "PASS: $script:assertionCount assertions. All 64 outcomes, payouts, invalid inputs, and 120,000 seeded symbol draws checked."
