using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace GildedRoseKata;

public class GildedRose(IList<Item> items)
{
    private const int QualityMin = 0;
    private const int QualityMax = 50;

    private readonly IList<Item> Items = items;

    /// <summary>
    /// Defines items by name that are immune to the ravages of time.
    /// </summary>
    private readonly HashSet<string> _ignoredItems =
    [
        "Sulfuras, Hand of Ragnaros"
    ];

    /// <summary>
    /// Custom update quality calculations by item name.
    /// </summary>
    private static readonly Dictionary<string, Func<Item, int>> CalculateQualityByItemName =
        new()
        {
            // Aged Brie
            // - Improves with age
            // - Cannot exceed quality max
            ["Aged Brie"] = item =>
            {
                // do nothing when max quality hit
                if (item.Quality >= QualityMax) 
                    return item.Quality;

                // improve quality
                var result = item.Quality + 1;

                // +1 for old, stinky cheese
                if (result < QualityMax && item.SellIn < 0)
                    result++;

                return result;
            },

            // Backstage Pass
            // - Improves with age
            // - Doubles with under 10 days left
            // - triples with under 5 days left
            ["Backstage passes to a TAFKAL80ETC concert"] = item =>
            {
                var result = item.Quality;
                if (item.SellIn < 0)
                {
                    // when sell in exceeded the passes are worthless
                    result = 0;
                }
                else if (result < QualityMax)
                {
                    result++;
                    // +1 when under 10 left to sell
                    if (result < QualityMax && item.SellIn < 10)
                        result++;
                    // +1 when under 5 days left to sell
                    if (result < QualityMax && item.SellIn < 5)
                        result++;
                }
                return result;
            },

            // Conjured Mana Cake
            // - Degrades twice as fast
            ["Conjured Mana Cake"] = item =>
            {
                // degrades quality based on min allowed
                static int Degrade(int quality) => quality >= QualityMin + 2
                                                        ? quality - 2
                                                        : quality - 1;

                // do nothing when min quality hit
                if (item.Quality <= QualityMin) 
                    return item.Quality;

                var result = Degrade(item.Quality);

                // degrade quality twice as fast when sell in exceeded
                if (item.SellIn < 0 && result > QualityMin)
                    result = Degrade(result);

                return result;
            }
        };

    /// <summary>
    /// Processes a single day of quality changes for all items.
    /// </summary>
    public void UpdateQuality()
    {
        // process items
        foreach (var item in Items)
        {
            // ignore items
            if (_ignoredItems.Contains(item.Name))
                continue;

            // another day...another quality update
            item.SellIn--;
            var calculateQuality = CalculateQualityByItemName.GetValueOrDefault(item.Name) 
                                        ?? DefaultCalculateQuality;
            item.Quality = Math.Clamp(
                                calculateQuality(item), 
                                QualityMin, 
                                QualityMax
                            );
        }
    }

    /// <summary>
    /// Default logic for updating a item's quality.
    /// - Quality cannot drop below 0
    /// - Quality degrades twice as fast when sell in has elapsed.
    /// </summary>
    private static int DefaultCalculateQuality(Item item)
    {
        if (item.Quality <= QualityMin) return item.Quality;

        var result = item.Quality - 1;

        // degrade quality twice as fast when sell in exceeded
        if (result > QualityMin && item.SellIn < 0)
            result--;

        return result;
    }
}
