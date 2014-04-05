﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace main
{

    class SuperRegionsLowerArmiesSorter : System.Collections.Generic.IComparer<SuperRegion>
    {
        public SuperRegionsLowerArmiesSorter() { }

        public int Compare(SuperRegion a, SuperRegion b)
        {
            return a.ArmiesReward - b.ArmiesReward;
        }
    }

    // this is only meant for round 0 analysis
    class SuperRegionsExpansionTargetSorter : System.Collections.Generic.IComparer<SuperRegion>
    {
        List<Region> picks;
        string myName;

        public SuperRegionsExpansionTargetSorter(List<Region> _picks, string _myName) {
            picks = _picks;
            myName = _myName;
        }

        public int Count(SuperRegion a)
        {
            // superregion is considered safe if we have all the strategic starting picks (in, or neighbouring) the superregion
            // this function quantifies how safe it really is

            int count = 0;

            bool redflag = false; //redflag when there is enemy or unknown on a starting pick in or neighbouring
            foreach (Region reg in picks)
            {
                //bool found = false;

                // check if we have any neighbour bordering this superregion (this selection includes picks in or countering)
                foreach(Region neighbour in reg.Neighbors) {
                    
                    if (neighbour.SuperRegion.Id == a.Id) {
                        //if it is neighboring an area of this superregion, check who the pick was assigned to
                        switch (reg.PlayerName)
                        {
                            case "player1":
                                if (myName == "player1")
                                {
                                    count += 2;
                                } else { 
                                    redflag = true;
                                }
                                break;
                            case "player2":
                                if (myName == "player2")
                                {
                                    count += 2;
                                } else { 
                                    redflag = true;
                                }
                                break;
                            case "neutral":
                                count++;
                                break;
                            case "unknown":
                                //todo: check if the pick was picked by me in lower position then my last gotten pick
                                //count--;
                                //todo: instead of giving it lower priority, it could also be worth increasing the aggressivity
                                count--;
                                break;
                        }
                    
                        // we only need to check one of the found neighbours to know this is a relevant pick, skip the rest
                        //found = true;
                        break;
                    }
                }

                //if (found) break;

            }

            if (a.SubRegions.Count <= 4) count += 2;
            else if (a.SubRegions.Count <= 5) count++;

            if (redflag) count = -1;
            
            return count;
        }

        public int Compare(SuperRegion a, SuperRegion b)
        {
            int ac = Count(a);
            int bc = Count(b);
            //Console.WriteLine(a.Id + " " + ac + " : " + b.Id + " " + bc);
            //if ((ac == -1) || (bc == -1)) return -1;

            return bc - ac;
        }
    }


    //todo: refactor this sorter to use on other rounds
   /* class SuperRegionsExpansionTargetSorter : System.Collections.Generic.IComparer<SuperRegion>
    {
        List<Region> picks;
        string myName;

        public SuperRegionsExpansionTargetSorter(List<Region> _picks, string _myName)
        {
            picks = _picks;
            myName = _myName;
        }

        public int Count(SuperRegion a)
        {
            // superregion is considered safe if we have all the strategic starting picks (in or neighbouring) the superregion
            // this function quantifies how safe it really is

            int count = 0;

            // no need to consider expanding into areas you already own
            if (a.OwnedByPlayer() == myName) return 0;

            // get list of all starting picks of this region
            bool redflag = false;
            foreach (Region reg in picks)
            {
                foreach (Region neighbour in reg.Neighbors)
                {
                    if (neighbour.SuperRegion.Id == a.Id)
                    {
                        switch (reg.PlayerName)
                        {
                            case "player1":
                                if (myName == "player1")
                                {
                                    count += 2;
                                }
                                else
                                {
                                    redflag = true;
                                }
                                break;
                            case "player2":
                                if (myName == "player2")
                                {
                                    count += 2;
                                }
                                else
                                {
                                    redflag = true;
                                }
                                break;
                            case "neutral":
                                count++;
                                break;
                            case "unknown":
                                //todo: check if the pick was picked by me in lower position then my last gotten pick
                                count--;
                                //todo: instead of giving it lower priority, it could also be worth increasing the aggressivity
                                break;
                        }

                        continue; //we only need to check one of the found neighbours
                    }
                }
            }

            if (redflag) count = 0;

            return count;
        }

        public int Compare(SuperRegion a, SuperRegion b)
        {
            return Count(a) - Count(b);
        }
    }*/





    class RegionsImportanceSorter : System.Collections.Generic.IComparer<Region>
    {
        private List<SuperRegion> list;

        public RegionsImportanceSorter(List<SuperRegion> _list) {
            list = _list;
        }

        public int Compare(Region a, Region b)
        {
            a.Neighbors.Sort(new RegionsNeighbouringSuperRegionSorter(list));
          
            // if neighbouring a higher ranked superregion
            if (a.SuperRegion.Id != a.Neighbors[0].Id)
            {
                b.Neighbors.Sort(new RegionsNeighbouringSuperRegionSorter(list));

                // prioritize neighbouring a good superregion
                return a.Neighbors[0].SuperRegion.ArmiesReward - b.Neighbors[0].SuperRegion.ArmiesReward;
            } else {
                // prioritize regions within higher ranked superregion
                return list.IndexOf(a.SuperRegion) - list.IndexOf(b.SuperRegion);
            }

            //todo: prioritize regions that are alone in good superregions, or we might not be there at all
        }
    }
    
    class RegionsNeighbouringSuperRegionSorter : System.Collections.Generic.IComparer<Region>
    {
        private List<SuperRegion> list;

        public RegionsNeighbouringSuperRegionSorter(List<SuperRegion> _list)
        {
            list = _list;
        }

        public int Compare(Region a, Region b)
        {
            return list.IndexOf(a.SuperRegion) - list.IndexOf(b.SuperRegion);
        }
    }

    class RegionsAvailableArmiesSorter : System.Collections.Generic.IComparer<Region>
    {
        string myName;

        public RegionsAvailableArmiesSorter(string _myName)
        {
            myName = _myName;
        }

        public int Count(Region a) {
            int aArmies = a.Armies + a.PledgedArmies - a.ReservedArmies;
            if (!a.OwnedByPlayer(myName)) aArmies = -1;
            return aArmies;
        }

        public int Compare(Region a, Region b)
        {
            int aArmies = Count(a);
            int bArmies = Count(b);

            return bArmies - aArmies;
        }
    }

    
    class RegionsMinimumExpansionSorter : System.Collections.Generic.IComparer<Region>
    {

        string myName;
        string opponentName;

        public RegionsMinimumExpansionSorter(string _myName, string _opponentName)
        {
            myName = _myName;
            opponentName = _opponentName;
        }

        public int Count(Region a)
        {
            int count = 0;

            if (a.OwnedByPlayer(opponentName)) { return -5; }
            if (a.OwnedByPlayer(myName)) { return -10; }

            foreach (Region neigh in a.Neighbors)
            {
                // if neighbor is the enemy, we shouldnt be thinking of expansion, we want to keep our stack close to the enemy
                if (neigh.OwnedByPlayer(opponentName)) {
                    count -= 5;
                }

                // the more neighbours belong to me the better
                if (neigh.OwnedByPlayer(myName))
                {
                    count += 3;
                }

                // if it has neutrals on the target superregion its good
                if (neigh.OwnedByPlayer("neutral") && (neigh.SuperRegion.Id == a.SuperRegion.Id)) count++;

                // if it has unknowns on the target superregion its even better (means we will be able to finish it faster)
                if (neigh.OwnedByPlayer("unknown") && (neigh.SuperRegion.Id == a.SuperRegion.Id)) count += 2;

                // if it has only has 1 army it costs less to take, so its better
                if (neigh.Armies == 1) count++;

                // boost if this territory can take this neighbour without deploying, at all
                int armyCount = a.Armies + a.PledgedArmies - a.ReservedArmies - neigh.Armies*2;
                // the more armies we'll have left the better
                if (armyCount > 0) count += armyCount;
            }
           
            return count;
        }

        public int Compare(Region a, Region b)
        {
            // push down region which is not a neutral
            //if (!a.OwnedByPlayer("neutral"))
            //{
            //    if (a.OwnedByPlayer(opponentName)) { return 0; }
            //    else return 1;
            //}

            // sort
            return Count(b) - Count(a);
        }
    }
    

    class RegionsBestExpansionAttackerSorter : System.Collections.Generic.IComparer<Region>
    {

        string myName;

        public RegionsBestExpansionAttackerSorter(string _myName)
        {
            myName = _myName;
        }

        public int Count(Region a)
        {
            int count = 0;

            // if it's not our territory, don't bother
            if (!a.OwnedByPlayer(myName)) return -1;

            // best attacker is the one with more armies available
            int armyCount = a.Armies + a.PledgedArmies - a.ReservedArmies;
            if (armyCount > 0) count += armyCount;
           
            return count;
        }

        public int Compare(Region a, Region b)
        {
            int ca = Count(a);
            int cb = Count(b);
            //Console.WriteLine(a.Id + " " + ca + " : " + b.Id + " " + cb);

            return cb - ca;
        }
    }

    
    class RegionsMoveLeftoversTargetSorter : System.Collections.Generic.IComparer<Region>
    {

        string myName;
        string opponentName;
        int expansionTarget;

        public RegionsMoveLeftoversTargetSorter( string _myname, string _opponentName, int _expansionTarget )
        {
            myName = _myname;
            opponentName = _opponentName;
            expansionTarget = _expansionTarget;
        }

        public int Count(Region a)
        {
            int count = 0;

            if (a.OwnedByPlayer("neutral") || a.OwnedByPlayer(opponentName))
            {
                return 0;
            }  

            // if not bordering an enemy
            // move leftover to where they can border an enemy
            // or finish the highest ranked expansion target superregion more easily

            // we can also give a little bonus if it's expanding into an area that will help finish the superregion
            foreach (Region neigh in a.Neighbors)
            {
                if (neigh.OwnedByPlayer(opponentName)) return 0;

                foreach (Region nextborder in neigh.Neighbors)
                {
                    if (nextborder.OwnedByPlayer(opponentName)) count += 2;

                    if (nextborder.OwnedByPlayer("neutral") && (nextborder.SuperRegion.Id == expansionTarget)) count++;
                }
            }

            return count;
        }

        public int Compare(Region a, Region b)
        {
            return Count(b) - Count(a);
        }
    }



    class RegionsHigherArmiesInMyNameSorter : System.Collections.Generic.IComparer<Region>
    {
        string myname;

        public RegionsHigherArmiesInMyNameSorter(string _myname) {
            myname = _myname;
        }

        public int Count(Region a)
        {
            int ac = 0;
            if (a.OwnedByPlayer(myname)) {
                ac+=a.Armies;
            }
            return ac;
        }

        public int Compare(Region a, Region b)
        {
            return Count(b) - Count(a);
        }
    }


}
