using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heap<T> where T : IHeapNode<T>
{
	private T[] nodes;
	int nodesCount;

	private int ParentIndex(int childIndex) => (childIndex - 1) / 2;
	private int LeftChildIndex(int parentIndex) => parentIndex * 2 + 1;
	private int RightChildIndex(int parentIndex) => parentIndex * 2 + 2;

	public Heap(int heapSize)
	{
		nodes = new T[heapSize];
	}

	public void Add(T node)
	{
		node.Index = nodesCount;
		nodes[nodesCount] = node;
		SortUp(node);
		nodesCount++;
	}

	public T RemoveFirst()
	{
		T firstNode = nodes[0];
		nodesCount--;
		nodes[0] = nodes[nodesCount];
		nodes[0].Index = 0;
		SortDown(nodes[0]);
		return firstNode;
	}

	public void UpdateNode(T node)
	{
		SortUp(node);
	}

	public int Count
	{
		get
		{
			return nodesCount;
		}
	}

	public bool Contains(T node)
	{
		return Equals(nodes[node.Index], node);
	}

	void SortDown(T node)
	{
		while (true)
		{
			int leftChildIndex = LeftChildIndex(node.Index);
			int rightChildIndex = RightChildIndex(node.Index);

			if (leftChildIndex < nodesCount)
			{
				int swapIndex = leftChildIndex;

				if (rightChildIndex < nodesCount)
				{
					if (nodes[leftChildIndex].CompareTo(nodes[rightChildIndex]) < 0)
					{
						swapIndex = rightChildIndex;
					}
				}

				if (node.CompareTo(nodes[swapIndex]) < 0)
				{
					Swap(node, nodes[swapIndex]);
				}
				else
				{
					return;
				}

			}
			else
			{
				return;
			}

		}
	}

	void SortUp(T node)
	{
		while (true)
		{
			T parentNode = nodes[ParentIndex(node.Index)];
			if (node.CompareTo(parentNode) > 0)
			{
				Swap(node, parentNode);
			}
			else
			{
				break;
			}
		}
	}

	void Swap(T nodeA, T nodeB)
	{
		nodes[nodeA.Index] = nodeB;
		nodes[nodeB.Index] = nodeA;
		int nodeAIndex = nodeA.Index;
		nodeA.Index = nodeB.Index;
		nodeB.Index = nodeAIndex;
	}
}

public interface IHeapNode<T> : IComparable<T>
{
    int Index { get; set; }
}