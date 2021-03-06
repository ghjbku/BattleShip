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
        public Ship[] _myShips { get; private set; }
        public Vector TargetCoordinates { get; private set; }
        public int Hits { get; private set; }

        public const int _numberOfShips = 5; // const is also static
        public static readonly int[] _shipLengths = new int[] { 2, 3, 3, 4, 5 };
        public static readonly Vector NO_TARGET = new Vector(-1, -1);

        public Player(string name, Ship[] newShips)
        {
            Name = name;
            _myTerritory = new int[10, 10];
            _myShips = newShips;
            PlaceMyShips();
            TargetCoordinates = NO_TARGET;
            Hits = 0;
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
                Hits++;
                //check if destroyed
                if (feedback[1])
                    OnEnemyShipDestroyed();
            } else
            {
                Logger.Log("Missed.");
            }

            TargetCoordinates = NO_TARGET;
        }

        public void AimAt(Vector target)
        {
            TargetCoordinates = target;
            Logger.Log(Name + " is aiming at " + TargetCoordinates.ToString());
        }

        public bool[] IsHitAndSink(Vector coordinate)
        {
            bool[] hit = new bool[] {false, false};
            Ship hitShip = null;

            foreach(Ship ship in _myShips)
            {
                if(ship.GotHitAt(coordinate))
                {
                    hit[0] = true;
                    hit[1] = ship.IsDestroyed();
                    hitShip = ship;
                    break;
                }
            }

            _myTerritory[coordinate.X, coordinate.Y] = hit[0] ? 3 : 1;
            if (hit[1])
                OnMyShipDestroyed(hitShip);

            return hit;
        }

        public virtual void OnMyShipDestroyed(Ship ship)
        {

            Logger.Log("On ship destroyed...");

            for (int y = ship.Coordinates[0].Y - 1; y <= ship.Coordinates[ship.Length - 1].Y + 1; y++)
            {
                for (int x = ship.Coordinates[0].X - 1; x <= ship.Coordinates[ship.Length - 1].X + 1; x++)
                {
                    if ((x > -1) && (x < 10) && (y > -1) && (y < 10))
                        _myTerritory[x, y] = 1;
                }
            }

            foreach (Vector pos in ship.Coordinates)
                _myTerritory[pos.X, pos.Y] = 3;
        }

        public virtual void OnEnemyShipDestroyed()
        {
            Logger.Log("Destroyed!");
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
