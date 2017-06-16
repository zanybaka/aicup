using System;
using System.Collections.Generic;
using System.Linq;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;
using SFML.System;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    /// <summary>
    /// http://russianaicup.ru/forum/index.php?topic=620.msg6125#msg6125
    /// </summary>
    public sealed class MyStrategy
    {
        static double WAYPOINT_RADIUS = 100.0D;

        static double LOW_HP_FACTOR = 0.25D;

        SfmlVisualizer p;

        /**
         * Ключевые точки для каждой линии, позволяющие упростить управление перемещением волшебника.
         * <p>
         * Если всё хорошо, двигаемся к следующей точке и атакуем противников.
         * Если осталось мало жизненной энергии, отступаем к предыдущей точке.
         */

        Random random = new Random();

        LaneType lane;

        Point2D[] waypoints;

        Wizard _myWizard;
        World _myWorld;
        Game _myGame;
        Move _myMove;

        internal MyStrategy(SfmlVisualizer p)
        {
            this.p = p;
        }

        /**
         * Основной метод стратегии, осуществляющий управление волшебником.
         * Вызывается каждый тик для каждого волшебника.
         *
         * @param self  Волшебник, которым данный метод будет осуществлять управление.
         * @param world Текущее состояние мира.
         * @param game  Различные игровые константы.
         * @param move  Результатом работы метода является изменение полей данного объекта.
         */

        public void Move(Wizard self, World world, Game game, Move move)
        {
            InitializeStrategy(self, game);
            InitializeTick(self, world, game, move);
            PaintGeneralInfo(self, world, game);
            p.PaintWayPoints(waypoints.Select(x => new Vector2f((float) x.GetX(), (float) x.GetY())).ToArray());

            // Постоянно двигаемся из-стороны в сторону, чтобы по нам было сложнее попасть.
            // Считаете, что сможете придумать более эффективный алгоритм уклонения? Попробуйте! ;)
            move.StrafeSpeed = (random.Next(2) == 1) ? game.WizardStrafeSpeed : -game.WizardStrafeSpeed;

            // Если осталось мало жизненной энергии, отступаем к предыдущей ключевой точке на линии.
            if (self.Life < self.MaxLife * LOW_HP_FACTOR)
            {
                GoTo(GetPreviousWaypoint());
                return;
            }

            LivingUnit nearestTarget = GetNearestTarget();

            // Если видим противника ...
            if (nearestTarget != null)
            {
                double distance = self.GetDistanceTo(nearestTarget);

                // ... и он в пределах досягаемости наших заклинаний, ...
                if (distance <= self.CastRange)
                {
                    double angle = self.GetAngleTo(nearestTarget);

                    // ... то поворачиваемся к цели.
                    move.Turn = angle;

                    // Если цель перед нами, ...
                    if (Math.Abs(angle) < game.StaffSector / 2.0D)
                    {
                        // ... то атакуем.
                        move.Action = ActionType.MagicMissile;
                        move.CastAngle = angle;
                        move.MinCastDistance = (distance - nearestTarget.Radius + game.MagicMissileRadius);
                    }

                    return;
                }
            }

            // Если нет других действий, просто продвигаемся вперёд.
            GoTo(GetNextWaypoint());
        }

        /**
         * Инциализируем стратегию.
         * <p>
         * Для этих целей обычно можно использовать конструктор, однако в данном случае мы хотим инициализировать генератор
         * случайных чисел значением, полученным от симулятора игры.
         */

        void InitializeStrategy(Wizard self, Game game)
        {
            //if (random == null)
            //{
            //  random = new Random(game.RandomSeed);

            double mapSize = game.MapSize;

            switch ((int)self.Id)
            {
                case 1:
                case 2:
                case 6:
                case 7:
                    lane = LaneType.Top;
                    waypoints = new Point2D[]
                    {
                        new Point2D(100.0D, mapSize - 100.0D),
                        new Point2D(100.0D, mapSize - 400.0D),
                        new Point2D(200.0D, mapSize - 800.0D),
                        new Point2D(200.0D, mapSize*0.75D),
                        new Point2D(200.0D, mapSize*0.5D),
                        new Point2D(200.0D, mapSize*0.25D),
                        new Point2D(200.0D, 200.0D),
                        new Point2D(mapSize*0.25D, 200.0D),
                        new Point2D(mapSize*0.5D, 200.0D),
                        new Point2D(mapSize*0.75D, 200.0D),
                        new Point2D(mapSize - 200.0D, 200.0D)
                    };
                    break;
                case 3:
                case 8:
                    lane = LaneType.Middle;
                    waypoints = new Point2D[]
                    {
                        new Point2D(100.0D, mapSize - 100.0D),
                        self.Faction == Faction.Renegades
                            ? new Point2D(600.0D, mapSize - 200.0D)
                            : new Point2D(200.0D, mapSize - 600.0D),
                        new Point2D(800.0D, mapSize - 800.0D),
                        new Point2D(mapSize - 600.0D, 600.0D)
                    };
                    break;
                case 4:
                case 5:
                case 9:
                case 10:
                    lane = LaneType.Bottom;
                    waypoints = new Point2D[]
                    {
                        new Point2D(100.0D, mapSize - 100.0D),
                        new Point2D(400.0D, mapSize - 100.0D),
                        new Point2D(800.0D, mapSize - 200.0D),
                        new Point2D(mapSize*0.25D, mapSize - 200.0D),
                        new Point2D(mapSize*0.5D, mapSize - 200.0D),
                        new Point2D(mapSize*0.75D, mapSize - 200.0D),
                        new Point2D(mapSize - 200.0D, mapSize - 200.0D),
                        new Point2D(mapSize - 200.0D, mapSize*0.75D),
                        new Point2D(mapSize - 200.0D, mapSize*0.5D),
                        new Point2D(mapSize - 200.0D, mapSize*0.25D),
                        new Point2D(mapSize - 200.0D, 200.0D)
                    };
                    break;
            }

            // Наша стратегия исходит из предположения, что заданные нами ключевые точки упорядочены по убыванию
            // дальности до последней ключевой точки. Сейчас проверка этого факта отключена, однако вы можете
            // написать свою проверку, если решите изменить координаты ключевых точек.

            /*Point2D lastWaypoint = waypoints[waypoints.length - 1];
    
            Preconditions.checkState(ArrayUtils.isSorted(waypoints, (waypointA, waypointB) -> Double.compare(
                    waypointB.getDistanceTo(lastWaypoint), waypointA.getDistanceTo(lastWaypoint)
            )));*/
            // }
        }

        /**
         * Сохраняем все входные данные в полях класса для упрощения доступа к ним.
         */

        private void InitializeTick(Wizard self, World world, Game game, Move move)
        {
            _myWizard = self;
            _myWorld = world;
            _myGame = game;
            _myMove = move;
        }

        /**
         * Данный метод предполагает, что все ключевые точки на линии упорядочены по уменьшению дистанции до последней
         * ключевой точки. Перебирая их по порядку, находим первую попавшуюся точку, которая находится ближе к последней
         * точке на линии, чем волшебник. Это и будет следующей ключевой точкой.
         * <p>
         * Дополнительно проверяем, не находится ли волшебник достаточно близко к какой-либо из ключевых точек. Если это
         * так, то мы сразу возвращаем следующую ключевую точку.
         */

        private Point2D GetNextWaypoint()
        {
            int lastWaypointIndex = waypoints.Length - 1;
            Point2D lastWaypoint = waypoints[lastWaypointIndex];

            for (int waypointIndex = 0; waypointIndex < lastWaypointIndex; ++waypointIndex)
            {
                Point2D waypoint = waypoints[waypointIndex];

                if (waypoint.GetDistanceTo(_myWizard) <= WAYPOINT_RADIUS)
                {
                    return waypoints[waypointIndex + 1];
                }

                if (lastWaypoint.GetDistanceTo(waypoint) < lastWaypoint.GetDistanceTo(_myWizard))
                {
                    return waypoint;
                }
            }

            return lastWaypoint;
        }

        /**
         * Действие данного метода абсолютно идентично действию метода {@code getNextWaypoint}, если перевернуть массив
         * {@code waypoints}.
         */

        private Point2D GetPreviousWaypoint()
        {
            Point2D firstWaypoint = waypoints[0];

            for (int waypointIndex = waypoints.Length - 1; waypointIndex > 0; --waypointIndex)
            {
                Point2D waypoint = waypoints[waypointIndex];

                if (waypoint.GetDistanceTo(_myWizard) <= WAYPOINT_RADIUS)
                {
                    return waypoints[waypointIndex - 1];
                }

                if (firstWaypoint.GetDistanceTo(waypoint) < firstWaypoint.GetDistanceTo(_myWizard))
                {
                    return waypoint;
                }
            }

            return firstWaypoint;
        }

        /**
         * Простейший способ перемещения волшебника.
         */

        private void GoTo(Point2D point)
        {
            double angle = _myWizard.GetAngleTo(point.GetX(), point.GetY());

            _myMove.Turn = angle;

            if (Math.Abs(angle) < _myGame.StaffSector / 4.0D)
            {
                _myMove.Speed = _myGame.WizardForwardSpeed;
            }
        }

        /**
         * Находим ближайшую цель для атаки, независимо от её типа и других характеристик.
         */

        private LivingUnit GetNearestTarget()
        {
            List<LivingUnit> targets = new List<LivingUnit>();

            targets.AddRange(_myWorld.Buildings);
            targets.AddRange(_myWorld.Wizards);
            targets.AddRange(_myWorld.Minions);

            LivingUnit nearestTarget = null;

            double nearestTargetDistance = Double.MaxValue;

            foreach (LivingUnit target in targets)
            {
                if (target.Faction == Faction.Neutral || target.Faction == _myWizard.Faction)
                {
                    continue;
                }

                double distance = _myWizard.GetDistanceTo(target);

                if (distance < nearestTargetDistance)
                {
                    nearestTarget = target;
                    nearestTargetDistance = distance;
                }
            }

            return nearestTarget;
        }

        private void PaintGeneralInfo(Wizard self, World world, Game game)
        {
            p.DrawCircle(self.X, self.Y, self.VisionRange, 0xC8C8C8);
            p.DrawCircle(self.X, self.Y, self.CastRange, 0xC8C8C8);
            p.DrawCircle(self.X, self.Y, game.FetishBlowdartAttackRange, 0xC8C8C8);
            string coord = string.Format("me: {0};{1}", (int)self.X, (int)self.Y);
            p.PrintMid(self.X, self.Y + self.Radius + 15, coord, "index: {0}", world.TickIndex);
            p.PrintMid(self.X, self.Y + self.Radius + 33, coord, coord);
        }

        /**
        * Вспомогательный класс для хранения позиций на карте.
        */

        public class Point2D
        {
            private readonly double _x;
            private readonly double _y;

            public Point2D(double x, double y)
            {
                _x = x;
                _y = y;
            }

            public double GetX()
            {
                return _x;
            }

            public double GetY()
            {
                return _y;
            }

            public double GetDistanceTo(double x, double y)
            {
                return Math.Sqrt(Math.Pow((_x - x), 2) + Math.Pow((_y - y), 2));
            }

            public double GetDistanceTo(Point2D point)
            {
                return GetDistanceTo(point.GetX(), point.GetY());
            }

            public double GetDistanceTo(Unit unit)
            {
                return GetDistanceTo(unit.X, unit.Y);
            }
        }
    }
}