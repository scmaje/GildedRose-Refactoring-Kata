using Xunit;
using System.Collections.Generic;
using GildedRoseKata;

namespace GildedRoseTests;

public class GildedRoseTest
{
    static Item CreateItem(string name, int sellIn, int quality) =>
        new() { Name = name, SellIn = sellIn, Quality = quality };

    static GildedRose CreateSystemUnderTest(params Item[] items) =>
        new([.. items]);

    const string AgedBrie = "Aged Brie"; // required item name
    const string BackstagePass = "Backstage passes to a TAFKAL80ETC concert"; // required item name
    const string ConjuredItem = "Conjured Mana Cake"; // required item name
    const string Sulfuras = "Sulfuras, Hand of Ragnaros"; // required item name

    [Theory] // for all non-sulfuras tests: name remains unchanged, sell in reduced by 1
    [InlineData(10, "foo", 10, 10, "foo", 9, 9)] // before expiration: quality reduced by 1
    [InlineData(20, "foo", 10, 0, "foo", 9, 0)] // before expiration: quality limited to 0
    [InlineData(30, "foo", 0, 10, "foo", -1, 8)] // after expiration: quality reduced by 2
    [InlineData(40, "foo", 0, 1, "foo", -1, 0)] // after expiration: quality limited to 0
    [InlineData(100, AgedBrie, 10, 10, AgedBrie, 9, 11)] // before expiration: quality increased by 1
    [InlineData(110, AgedBrie, 10, 50, AgedBrie, 9, 50)] // before expiration: quality limited to 50
    [InlineData(120, AgedBrie, 0, 10, AgedBrie, -1, 12)] // after expiration: quality increased by 2
    [InlineData(130, AgedBrie, 0, 49, AgedBrie, -1, 50)] // after expiration: quality limited to 50
    [InlineData(200, BackstagePass, 20, 10, BackstagePass, 19, 11)] // before 10 days to expiration: quality increased by 1
    [InlineData(210, BackstagePass, 20, 50, BackstagePass, 19, 50)] // before 10 days to expiration: quality limited to 50
    [InlineData(220, BackstagePass, 10, 10, BackstagePass, 9, 12)] // when 10 days to expiration: quality increased by 2
    [InlineData(230, BackstagePass, 10, 49, BackstagePass, 9, 50)] // when 10 days to expiration: quality limited to 50
    [InlineData(240, BackstagePass, 5, 10, BackstagePass, 4, 13)] // when 5 days to expiration: quality increased by 3
    [InlineData(250, BackstagePass, 5, 48, BackstagePass, 4, 50)] // when 5 days to expiration: quality limited to 50
    [InlineData(260, BackstagePass, 0, 10, BackstagePass, -1, 0)] // after expiration: quality dropped to 0
    [InlineData(300, ConjuredItem, 10, 10, ConjuredItem, 9, 8)] // before expiration: quality reduced by 2
    [InlineData(310, ConjuredItem, 10, 1, ConjuredItem, 9, 0)] // before expiration: quality limited to 0
    [InlineData(320, ConjuredItem, 0, 10, ConjuredItem, -1, 6)] // after expiration: quality reduced by 4
    [InlineData(330, ConjuredItem, 0, 3, ConjuredItem, -1, 0)] // after expiration: quality limited to 0
    [InlineData(400, Sulfuras, 10, 10, Sulfuras, 10, 10)] // nothing should change
    [InlineData(410, Sulfuras, 0, 10, Sulfuras, 0, 10)] // nothing should change
    public void Update_Quality_Tests(int id, string name, int sellIn, int quality, string expectedName, int expectedSellIn, int expectedQuality)
    {
        var item = CreateItem(name, sellIn, quality);
        var sut = CreateSystemUnderTest(item);

        sut.UpdateQuality();

        Assert.Equal(expectedName, item.Name);
        Assert.Equal(expectedSellIn, item.SellIn);
        Assert.Equal(expectedQuality, item.Quality);
    }
}