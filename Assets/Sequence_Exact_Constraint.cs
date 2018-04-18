using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//This class defines a constraint which must appear in its slot ONLY ONCE IN SEQUENCE in either normal or reversed order.

public class Sequence_Exact_Constraint : Constraint
{
    private readonly List<_colors> sequence;
    private readonly List<_colors> sequence_reversed;

    public Sequence_Exact_Constraint(List<_colors> sequence)
    {
        this.sequence = sequence;
        List<_colors> sequence_reversed = new List<_colors>();
        for (int i = sequence.Count - 1; i >= 0; i--)
            sequence_reversed.Add(sequence[i]);
        this.sequence_reversed = sequence_reversed;
    }

    public List<List<Cell>> enumeratePlacementsInSlot(List<Cell> slot)
    {
        List<List<Cell>> possible_placements = this.getPossiblePlacements(slot, this.sequence);
        if (!this.sequence.SequenceEqual(this.sequence_reversed))
        {
            possible_placements.AddRange(this.getPossiblePlacements(slot, this.sequence_reversed));
        }
        List<List<Cell>> possible_valid_placements = new List<List<Cell>>();
        foreach (List<Cell> possible_placement in possible_placements)
        {
            if (this.isValidInSlot(possible_placement))
                possible_valid_placements.Add(possible_placement);
        }
        return possible_valid_placements;
    }

    private List<List<Cell>> getPossiblePlacements(List<Cell> slot, List<_colors> seq)
    {
        List<List<Cell>> possible_placements = new List<List<Cell>>();
        for (int i = 0; i < 6 - seq.Count + 1; i++)
        {
            List<Cell> possible_placement = new List<Cell>();
            for (int start = 0; start < i; start++)
            {
                possible_placement.Add(new Cell(slot[start].getColor()));
            }
            for (int j = i; j < i + seq.Count; j++)
            {
                if (!slot[j].getColor().Equals(_colors.U) && !slot[j].getColor().Equals(seq[j - i]))
                    break;
                else
                    possible_placement.Add(new Cell(seq[j - i]));
            }
            for (int end = i + seq.Count; end < 6; end++)
            {
                possible_placement.Add(new Cell(slot[end].getColor()));
            }
            if (possible_placement.Count() == 6)
            {
                possible_placements.Add(possible_placement);
            }
        }
        return possible_placements;
    }

    public bool isValidInSlot(List<Cell> slot) {
        return validStatesInSlot(slot) == 1;
    }

    public bool isOverValidInSlot(List<Cell> slot) {
        return validStatesInSlot(slot) > 1;
    }

    private int validStatesInSlot(List<Cell> slot)
    {
        int found_matches = 0;
        for (int i = 0; i < 6 - this.sequence.Count + 1; i++)
        {
            List<_colors> match = new List<_colors>();
            for (int j = i; j < i + this.sequence.Count; j++)
            {
                match.Add(slot[j].getColor());
            }
            if (match.SequenceEqual(this.sequence) || match.SequenceEqual(this.sequence_reversed))
                found_matches++;
        }
        return found_matches;
    }

    public bool Equals(Constraint constraint)
    {
        return constraint.GetType() == typeof(Sequence_Exact_Constraint) && constraint.getPattern().SequenceEqual(this.sequence);
    }

    public List<_colors> getPattern()
    {
        return this.sequence;
    }

    public string getPatternAsString()
    {
        return string.Join("", sequence.ToArray().Select(item => item.ToString()).ToArray());
    }
}


