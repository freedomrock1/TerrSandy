using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Terr01
{
    public class Device
    {
        // name
        public int did;
        public string name;
        public DeviceType type;
        public MapLocation loc;
        public NetworkInfo net;


        // type
            // router, workstation , switch, bridge, firewall 
        // network info?

    }

    public class Router : Device
    {

    }

    public class WorkStation : Device
    {

    }

    public class Printer : Device
    {

    }

    public class Switch : Device
    {

    }

    public class Bridge : Device
    {

    }

    public class Firewall : Device
    {

    }


    public class MapLocation
    {
        public 
        int x, y, z;
        public int room;

        public MapLocation()
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;
            this.room = 0;

        }
        public MapLocation(int x, int y, int z, int room) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.room = room;
            
        }

    }

    public class Map
    {
        public 
        int width, height, rooms;
        public ArrayList Nodes;

    }

    public enum DeviceType
    {
        Router,
        WorkStation,
        Printer,
        Switch,
        Bridge,
        Firewall
    }

    public class NetworkInfo
    {
        //int[] ip;
        public byte[] ip;
        public string mac;
        public byte[] mask;


    }

    public class Connection
    {
        public Device one;
        public Device two;

    }

    public class Layer3Switch:Switch
    {

    }

    public class RouterAdvanced : Router
    {
    }
    // physical info
    // network ID

}
