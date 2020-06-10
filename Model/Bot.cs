﻿using System;
using System.Collections.Generic;
using System.Text;
using Model.Data;

namespace Model
{
    public class Bot : Player
    {

        private static readonly Random _random = new Random();
        private bool _targetProbablyHorizontal; // reset to true on destroyed ship;
        private bool _firstHit;
        private List<Vector> _unresolvedShots;

        public Bot(string name) : base($"Bot {name}", GenerateShips())
        {
            _targetProbablyHorizontal = true;
            _firstHit = true;
            _unresolvedShots = new List<Vector>();
        }

        public static Ship[] GenerateShips()
        {
            Ship[] newShips = new Ship[_numberOfShips];

            int i = 0;
            bool again;
            do
            {
                again = false;

                newShips[i] = new Ship(_shipLengths[i]);
                newShips[i].IsHorizontal = _random.NextDouble() >= 0.5;
                newShips[i].Replace(new Vector(_random.Next(10 - newShips[i].Length), _random.Next(10 - newShips[i].Length)));

                for (int j = 0; j < i; j++)
                {
                    if(Ship.CollisionDetection(newShips[i], newShips[j]))
                    {
                        again = true;
                        break;
                    }
                        
                }
                if (!again)
                    i++;

            } while (i < 5);

            return newShips;
        }

        public void AutoAim()
        {
            Logger.Log("Auto aiming...");

            if(_unresolvedShots.Count == 0)
            {
                //random shot
                Logger.Log("Aiming at random location.");
                AimAt(new Vector(_random.Next(10), _random.Next(10)));
            } else
            {
                Vector lastHit = _unresolvedShots[_unresolvedShots.Count - 1];

                if (_targetProbablyHorizontal)
                {
                    // check right and left
                    if(lastHit.X < 9)
                    {
                        if (_enenmyTerritory[lastHit.X + 1, lastHit.Y] == 0)
                        {
                            AimAt(new Vector(lastHit.X + 1, lastHit.Y));
                            _firstHit = false;
                            return;
                        }
                    }

                    if (lastHit.X > 0)
                    {
                        if (_enenmyTerritory[lastHit.X - 1, lastHit.Y] == 0)
                        {
                            AimAt(new Vector(lastHit.X - 1, lastHit.Y));
                            _firstHit = false;
                            return;
                        }
                    }

                }
                else
                {
                    // check up and down
                    if (lastHit.Y < 9)
                    {
                        if (_enenmyTerritory[lastHit.X, lastHit.Y + 1] == 0)
                        {
                            AimAt(new Vector(lastHit.X, lastHit.Y + 1));
                            _firstHit = false;
                            return;
                        }
                    }

                    if (lastHit.Y > 0)
                    {
                        if (_enenmyTerritory[lastHit.X, lastHit.Y - 1] == 0)
                        {
                            AimAt(new Vector(lastHit.X, lastHit.Y - 1));
                            _firstHit = false;
                            return;
                        }
                    }

                }
                
                if (_firstHit)
                {
                    _targetProbablyHorizontal = !_targetProbablyHorizontal;
                    _firstHit = false;
                }
                else
                {
                    _unresolvedShots.Remove(lastHit);
                }

            }

            AutoAim();
        }



        public override void Shoot(Player enemy)
        {
            if (TargetCoordinates == NO_TARGET)
            {
                AutoAim();
            }

            Vector temp = TargetCoordinates;
            base.Shoot(enemy);

            if (_enenmyTerritory[temp.X, temp.Y] == 3)
                _unresolvedShots.Add(temp);
        }

        public override void OnShipDestroyed()
        {
            base.OnShipDestroyed();
            _firstHit = true;
            //_unresolvedShots.Clear();
        }

    }
}
