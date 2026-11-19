using System;

public static class SlotMachineRules
{
    public const int ReelCount = 3;
    public const int SymbolCount = 4;

    private static readonly int[] PayoutMultipliers = { 3, 2, 5, 10 };

    public static int[] CreateOutcome(Random random)
    {
        if (random == null)
        {
            throw new ArgumentNullException("random");
        }

        int[] outcome = new int[ReelCount];
        for (int i = 0; i < outcome.Length; i++)
        {
            outcome[i] = random.Next(SymbolCount);
        }

        return outcome;
    }

    public static bool IsWinningCombination(int[] outcome)
    {
        ValidateOutcome(outcome);
        return outcome[0] == outcome[1] && outcome[1] == outcome[2];
    }

    public static int GetWinnings(int[] outcome, int bet)
    {
        ValidateOutcome(outcome);
        if (bet <= 0)
        {
            throw new ArgumentOutOfRangeException("bet", "The bet must be greater than zero.");
        }

        if (!IsWinningCombination(outcome))
        {
            return 0;
        }

        return bet * PayoutMultipliers[outcome[0]];
    }

    private static void ValidateOutcome(int[] outcome)
    {
        if (outcome == null)
        {
            throw new ArgumentNullException("outcome");
        }

        if (outcome.Length != ReelCount)
        {
            throw new ArgumentException("An outcome must contain exactly three reel symbols.", "outcome");
        }

        for (int i = 0; i < outcome.Length; i++)
        {
            if (outcome[i] < 0 || outcome[i] >= SymbolCount)
            {
                throw new ArgumentOutOfRangeException("outcome", "Each symbol index must be between zero and three.");
            }
        }
    }
}
