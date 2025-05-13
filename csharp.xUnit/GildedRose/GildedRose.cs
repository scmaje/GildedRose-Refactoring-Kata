using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GildedRoseKata;

public class GildedRose(
    IList<Item> items, 
    HashSet<string> ignoredItems = null, 
    Action<Item> defaultUpdateQualityHandler = null, 
    Dictionary<string, Action<Item>> customUpdateQualityHandlers = null
)
{
    const int QUALITY_MIN = 0;
    const int QUALITY_MAX = 50;

    readonly IList<Item> Items = items;

    readonly HashSet<string> IgnoredItems = ignoredItems ??
    [
        "Sulfuras, Hand of Ragnaros"
    ];

    readonly Action<Item> DefaultUpdateQualityHandler = defaultUpdateQualityHandler ?? (item =>
    {
        UpdateQualityBy(item, -1);
        // degrade quality twice as fast when expired
        if (item.SellIn < 0)
            UpdateQualityBy(item, -1);
    });

    readonly Dictionary<string, Action<Item>> CustomUpdateQualityHandlers = customUpdateQualityHandlers ?? new()
    {
        // aged brie
        // - improves with age
        ["Aged Brie"] = item =>
        {
            if (item.Quality < 50)
            {
                UpdateQualityBy(item, 1);
                // quality boost for really old cheese
                if (item.SellIn < 0)
                    UpdateQualityBy(item, 1);
            }
        },

        // backstage pass
        // - improves with age
        // - doubles with under 10 days left
        // - triples with under 5 days left
        ["Backstage passes to a TAFKAL80ETC concert"] = item =>
        {
            if (item.SellIn < 0)
            {
                // sellin date passed, tickets are worthless
                item.Quality = 0;
            }
            else if (item.Quality < 50)
            {
                UpdateQualityBy(item, 1);
                // quality boost under 10 days out
                if (item.SellIn < 10)
                    UpdateQualityBy(item, 1);
                // quality boost under 5 days out
                if (item.SellIn < 5)
                    UpdateQualityBy(item, 1);
            }
        },

        // conjured item
        // - degrades twice as fast
        ["Conjured Mana Cake"] = item =>
        {
            UpdateQualityBy(item, -2);
            if (item.SellIn < 0)
                UpdateQualityBy(item, -2);
        }
    };

    public void UpdateQuality()
    {
        // snapshot items
        Item[] snapshot;
        lock (Items)
            snapshot = [.. Items];

        // parallel process items
        Parallel.ForEach(snapshot, UpdateQuality);
    }

    private void UpdateQuality(Item item)
    {
        if (!IgnoredItems.Contains(item.Name))
        {
            lock (item)
            {
                // reduce sell days remaining
                item.SellIn--;

                // lookup update quality handler for this item
                var updateQualityHandler = CustomUpdateQualityHandlers
                                                .GetValueOrDefault(
                                                    item.Name, 
                                                    DefaultUpdateQualityHandler
                                                );

                // invoke update quality handler
                updateQualityHandler(item);
            }
        }
    }

    private static void UpdateQualityBy(Item item, int value) =>
        item.Quality = Math.Clamp(item.Quality + value, QUALITY_MIN, QUALITY_MAX);
}
