using System;
using System.Collections.Generic;
using UnityEngine;

public class RXStockpile<T>
{
	//make a stockpile of equal value
	static public RXStockpile<T> CreateEqual(params T[] things)
	{
		RXStockpile<T> stockpile = new RXStockpile<T>();

		int thingCount = things.Length;
		for(int t = 0; t<thingCount; t++)
		{
			stockpile.AddItem(things[t],1.0f);
		}

		return stockpile;
	}

	public List<Item>items = new List<Item>();

	public RXStockpile()
	{
		
	}

	public void ResetLiveWeights()
	{
		int itemCount = items.Count;
		for(int i = 0; i<itemCount; i++)
		{
			items[i].liveWeight = items[i].weight;
		}
	}

	public T GetRandomItem()
	{
		return GetRandomItem(false);
	}

	public T GetRandomItem(bool shouldAdjustWeights)
	{
		float totalWeight = 0.0f;
		
		T foundThing = default(T);

		int itemCount = items.Count;
		
		for (int i = 0; i<itemCount; i++)
		{
			totalWeight += items[i].liveWeight;
		}

		float randomWeight = RXRandom.Float()*totalWeight;

		totalWeight = 0; //we'll use the same var to measure cumulative total weight

		for(int i = 0; i<itemCount; i++)
		{
			totalWeight = totalWeight + items[i].liveWeight;
			if(randomWeight <= totalWeight)
			{
				foundThing = items[i].thing;
				if(shouldAdjustWeights) items[i].liveWeight = 0;
				break;
			}
		}

		if(shouldAdjustWeights)
		{
			for (int i = 0; i<itemCount; i++)
			{
				Item item = items[i];
				item.liveWeight += item.weight;
			}
		}
		
		return foundThing;
	}

	public void ForceAdjustWeights(T thing) //this will adjust the weights as if the thing was picked
	{
		int itemCount = items.Count;
		for (int i = 0; i<itemCount; i++)
		{
			Item item = items[i];
			if(item.thing.Equals(thing))
			{
				item.liveWeight = item.weight; //reset
			}
			else 
			{
				item.liveWeight += item.weight; //increment
			}
		}
	}

	public float GetTotalWeight()
	{
		float totalWeight = 0;
		for (int i = 0; i<items.Count; i++)
		{
			totalWeight += items[i].liveWeight;
		}
		return totalWeight;
	}

	//for chaining
	public RXStockpile<T> Add(T thing, float weight)
	{
		AddItem(thing,weight);
		return this;
	}
	
	public Item AddItem(T thing, float weight)
	{
		Item item = new Item(thing, weight);
		items.Add(item);
		return item;
	}

	public Item GetItem(T thing)
	{
		for (int i = 0; i<items.Count; i++)
		{
			Item item = items[i];
			if(item.thing.Equals(thing)) 
			{
				return item;
			}
		}

		return null;
	}

	public class Item
	{
		public T thing;
		public float weight;
		public float liveWeight;
		
		public Item(T thing, float weight)
		{
			this.thing = thing;
			this.weight = weight;
			this.liveWeight = weight;
		}
	}
}