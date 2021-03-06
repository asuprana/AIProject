﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathfinding
{
  public class Tuple<T1, T2>
  {
    public T1 Item1 { get; private set; }
    public T2 Item2 { get; private set; }
    internal Tuple(T1 first, T2 second)
    {
      Item1 = first;
      Item2 = second;
    }
  }

  public static class Debug
  {
    public delegate void _print(object p);
    public static _print Log = new _print(Console.WriteLine);
    public static _print LogError = new _print(Console.WriteLine);
  }

  public class GridMap
  {
    //public HashSet<Location> walls = new HashSet<Location>();
    public bool[,] walls;
    public readonly int width;
    public readonly int height;
    public static readonly Location[] DIRS = new[]
    {
      new Location(-1,  1),
      new Location( 0,  1),
      new Location( 1,  1),
      new Location(-1,  0),
      new Location( 1,  0),
      new Location(-1, -1),
      new Location( 0, -1),
      new Location( 1, -1)
  };


    public GridMap(int width, int height) { this.width = width; this.height = height; }
    public GridMap(string path)
    {
        Debug.Log("Deserializing Map: " + path);

      string prevLine = "";
      string line;
      int ySize = -1; // start on -1 for board size data on the first line
      int xSize = 0;
      if (!File.Exists(path))
      {
        Debug.LogError("File does not exist");
        width = -1;
        return;
      }

      System.IO.StreamReader file = new System.IO.StreamReader(path);


      while ((line = file.ReadLine()) != null)
      {
        //skip first + second line for row length checking (first line storing board size data)
        if (ySize > 0)
        {
          //Check if all the length are even, 
          if (prevLine.Length != line.Length)
          {
            Debug.LogError("Error: Invalid map, uneven rowsize, row#: " + ySize);
            return;
          }
        }

        //Deserialize datas
        if (ySize == -1) // get board size data
        {
          string[] boardSize = line.Split(' ');

          this.width = Int32.Parse(boardSize[0]);
          this.height = Int32.Parse(boardSize[1]);
          walls= new bool[this.width, this.height];
        }
        else
        {
          string[] terrainType = line.Split(' ');
          int x = 0;
          foreach (string i in terrainType)
          {
            if (Int32.Parse(i) == 1)
              walls[x,ySize] = true;
            else
              walls[x, ySize] = false;
            ++x;
          }
        }

        prevLine = line; // to check if the map is rectangular, prevline length != line length when board is not rectangle
        ySize++; // move to next line
        xSize = (line.Length + 1) / 2; //get the last line length as map width (+1 /2 to remove spaces counts)
      }

      file.Close();
    }
    public bool InBound(Location i)
    {
      return i.x >= 0 && i.y >= 0 && i.x < width && i.y < height;
    }

    public bool InBound(int x, int y)
    {
      return x >= 0 && y >= 0 && x < width && y < height;
    }

    public bool IsWall(Location i)
    {

      bool outBound = !InBound(i);
      if (outBound)
        return false;

      bool isWall = walls[i.x, i.y];

      //iswall if this tile is a wall or is out of bound;
      return isWall;
    }
    public bool IsWall(int x, int y)
    {

      bool outBound = !InBound(x,y);
      if (outBound)
        return false;

      bool isWall = walls[x, y];
      //iswall if this tile is a wall or is out of bound;
      return isWall;
    }
    public float Cost(Location a, Location b)
    {
      bool isdiagonal = IsDiagonal(a, b);
      if (isdiagonal)
        return 1.4142135623f;
      else
        return 1;
    }
    public IEnumerable<Location> Neighbors(Location i)
    {
      List<Location> n = new List<Location>();
      foreach (Location dir in DIRS)
      {
        Location neighbor = new Location(i + dir);
        //non-diagonal
        if (!IsDiagonal(dir))
        {
          if (!IsWall(neighbor))
            yield return neighbor;
        }
        //diagonal
        else
        {
          bool horizontal = IsWall(new Location(i.x + dir.x, i.y));
          bool vertical = IsWall(new Location(i.x, i.y + dir.y));
          if (!horizontal && !vertical)
          {
            if (!IsWall(neighbor))
              yield return neighbor;
          }
        }
      }
    }
    public List<Location> NeighborsList(Location i)
    {
      List<Location> n = new List<Location>();
      foreach (Location dir in DIRS)
      {
        Location neighbor = new Location(i + dir);
        //non-diagonal
        if (!IsDiagonal(dir))
        {
          if (!IsWall(neighbor))
            n.Add(neighbor);
        }
        //diagonal
        else
        {
          bool horizontal = IsWall(new Location(i.x + dir.x, i.y));
          bool vertical = IsWall(new Location(i.x, i.y + dir.y));
          if (!horizontal && !vertical)
          {
            if (!IsWall(neighbor))
              n.Add(neighbor);
          }
        }
      }
      return n;
    }

    public static bool IsEmpty(IEnumerable<Location> en)
    {
      foreach (var c in en) { return false; }
      return true;
    }
    public void PrintGridMap()
    {
      Debug.Log("------CurrentGridMap-------");
      for (int y = 0; y < width; ++y)
      {
        string currentLine = "";
        for (int x = 0; x < width; ++x)
        {
          if (!IsWall(new Location(x, y)))
            currentLine += ("0 ");
          else
            currentLine += ("1 ");
        }
        Debug.Log(currentLine);
      }
    }

    private bool IsDiagonal(Location a, Location b)
    {
      Location dir = a - b;
      return Math.Abs(dir.x) == Math.Abs(dir.y);
    }
    private bool IsDiagonal(Location dir)
    {
      return Math.Abs(dir.x) == Math.Abs(dir.y);
    }
  }

  public struct Location
  {
    public readonly int x;
    public readonly int y;
    public Location(Location i) { x = i.x; y = i.y; }
    public Location(int x, int y) { this.x = x; this.y = y; }
    public static Location operator -(Location a, Location b) { return new Location(a.x - b.x, a.y - b.y); }
    public static Location operator +(Location a, Location b) { return new Location(a.x + b.x, a.y + b.y); }
    public bool Equals(Location a) { return a.x == this.x && a.y == this.y; }
    public void Print()
    {
      Debug.Log("(" + x + "," + y + ")");
    }
  }

  public class PriorityQueue<T>
  {
    //CHANGE LIST TO HEAP 
    private List<Tuple<T, double>> elements = new List<Tuple<T, double>>();

    public void Clear()
    {
      elements.Clear();
    }

    public int Count
    {
      get { return elements.Count; }
    }

    public void Enqueue(T item, double priority)
    {
      elements.Add(new Tuple<T, double>(item, priority));
    }

    public T Dequeue()
    {
      int bestIndex = 0;

      for (int i = 0; i < elements.Count; i++)
      {
        if (elements[i].Item2 < elements[bestIndex].Item2)
        {
          bestIndex = i;
        }
      }

      T bestItem = elements[bestIndex].Item1;
      elements.RemoveAt(bestIndex);
      return bestItem;
    }
  }

  public class AStarSearch
  {
    PriorityQueue<Location> openList = new PriorityQueue<Location>();
    GridMap map;
    public AStarSearch(GridMap map)
    {
      this.map = map;
    }

    public void Search(Location start, Location goal)
    {
      result.Clear();
      parent.Clear();
      openList.Clear();
      gVal.Clear();

      if (!map.InBound(goal))
        return;

      //Debug.Log("Search Started...");
      //Debug.Log("--startPoint--");
      //start.Print();
      //Debug.Log("--goalPoint--");
      //goal.Print();

      //push start node into openList
      openList.Enqueue(start, 0); // weight 0 at the begining;

      parent[start] = start;
      gVal[start] = 0;

      while (openList.Count > 0)
      {
        //pop cheapest from openlist
        Location current = openList.Dequeue();
        if (current.Equals(goal))
        {
          //Path found
          //traverse trhough
          Location p = current;
          while (!p.Equals(start))
          {
            result.Push(p);
            p = parent[p];
          }
          break;
        }

        IEnumerable<Location> neighbors = map.Neighbors(current);

        foreach (var neighbor in neighbors)
        {

          float cost = gVal[current] + map.Cost(current, neighbor);
          if (!gVal.ContainsKey(neighbor) || cost < gVal[neighbor])
          {
            gVal[neighbor] = cost;
            double priority = cost + heuristicFn(neighbor, goal);
            openList.Enqueue(neighbor, priority);
            parent[neighbor] = current;
          }
        }
      }

    }
    public void PrintResult()
    {
      Debug.Log("CurrentPath:");
      foreach (var p in result)
      {
        Debug.Log("(" + p.x + "," + p.y + ")");
      }
    }
    public Stack<Location> GetResult()
    {
      Stack<Location> myResult = result;
      return result;
    }

    private Dictionary<Location, Location> parent = new Dictionary<Location, Location>();
    private Dictionary<Location, float> gVal = new Dictionary<Location, float>();
    private Stack<Location> result = new Stack<Location>();

    private delegate float HeuristicFn(Location a, Location b);

    private HeuristicFn heuristicFn = new HeuristicFn(DefaultHeuristicFn);

    private static float DefaultHeuristicFn(Location a, Location b)
    {
      Location dir = b - a;
      return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public IEnumerable<Location> GetExploredList()
    {
      foreach (Location i in gVal.Keys)
      {
        yield return i;
      }
    }


    private void PrintNeighbors(IEnumerable<Location> l)
    {
      Debug.Log("--------Current Neighbor-----");
      if (GridMap.IsEmpty(l))
        Console.WriteLine("Empty");
      foreach (Location i in l)
      {
        i.Print();
      }
      Console.WriteLine();
    }

  }


}