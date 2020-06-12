﻿using System;
using System.Collections.Generic;
using System.Text;
using Model.Data;

namespace Model
{
    public class Player
    {

        public string Name { get; }

        /**
         * 0 - blank 
         * 1 - missed shot
         * 2 - ship
         * 3 - hit
         */
        private int[,] _myTerritory;
        public int[,] _enenmyTerritory { get; private set; }
        public Ship[] _myShips { get; private set; }
        public Vector TargetCoordinates { get; private set; }

        public const int _numberOfShips = 5; // const is also static
        public static readonly int[] _shipLengths = new int[] { 2, 3, 3, 4, 5 };
        public static readonly Vector NO_TARGET = new Vector(-1, -1);

        public Player(string name, Ship[] newShips)
        {
            Name = name;
            _enenmyTerritory = new int[10, 10];
            _myTerritory = new int[10, 10];
            _myShips = newShips;
            PlaceMyShips();
            TargetCoordinates = NO_TARGET;

        }

        void PlaceMyShips()
        {
            foreach(Ship ship in _myShips)
            {
                foreach(Vector v in ship.Coordinates)
                {
                    _myTerritory[v.X, v.Y] = 2;
                }
            }
        }

        public virtual void Shoot(Player enemy)
        {
            Logger.Log($"Shooting {enemy.Name}'s ship at {TargetCoordinates.ToString()}");

            if(TargetCoordinates == NO_TARGET) // this is not so necessary
            {
                Logger.Log("Invalid target!");
                return;
            }

            // check if it hit the enemy
            bool[] feedback = enemy.IsHitAndSink(TargetCoordinates);

            if(feedback[0])
            {
                Logger.Log("Hit!");
                _enenmyTerritory[TargetCoordinates.X, TargetCoordinates.Y] = 3;
                //check if destroyed
                if (feedback[1])
                    OnShipDestroyed(_enenmyTerritory, TargetCoordinates);
            } else
            {
                Logger.Log("Missed.");
                _enenmyTerritory[TargetCoordinates.X, TargetCoordinates.Y] = 1;
            }

            TargetCoordinates = NO_TARGET;
        }

        public void AimAt(Vector target)
        {
            // do a check here or before this to dont shoot twice at a target
            TargetCoordinates = target;
            Logger.Log(Name + " is aiming at " + TargetCoordinates.ToString());
        }

        public bool[] IsHitAndSink(Vector coordinate)
        {
            //bool hit = _myTerritory[coordinate.X, coordinate.Y] == 2;
            //_myTerritory[coordinate.X, coordinate.Y] = hit ? 3 : 1;
            bool[] hit = new bool[2];

            foreach(Ship ship in _myShips)
            {
                if(ship.GotHitAt(coordinate))
                {
                    hit[0] = true;
                    hit[1] = ship.IsDestroyed();
                    break;
                }
            }

            _myTerritory[coordinate.X, coordinate.Y] = hit[0] ? 3 : 1;
            if (hit[1])
                OnShipDestroyed(_myTerritory, coordinate);

            return hit;
        }

        public virtual void OnShipDestroyed(int[,] territory, Vector target)
        {
            Vector[] sideCoords = new Vector[] { Vector.Up, new Vector(1, -1), Vector.Right, new Vector(1, 1), Vector.Down, new Vector(-1, 1), Vector.Left, new Vector(-1, -1) };
            List<Vector> l = new List<Vector>();
            l.Add(target);

            do
            {
                Vector current = l[0];
                foreach(Vector v in sideCoords)
                {
                    Vector temp = current + v;
                    if(territory[temp.X, temp.Y] == 3)
                    {
                        l.Add(temp);
                    } else if (territory[temp.X, temp.Y] == 0)
                    {
                        territory[temp.X, temp.Y] = 1;
                    }
                }

                l.Remove(current);
            } while (l.Count > 0);
        }







        public int[,] GetTerritory(bool showHidden = false)
        {
            int[,] output = (int[,])_myTerritory.Clone();
            if(!showHidden)
            {
                for (int i = 0; i < 10; i++)
                    for (int j = 0; j < 10; j++)
                        if (output[i, j] == 2)
                            output[i, j] = 0;
            }

            return output;
        }

    }
}
