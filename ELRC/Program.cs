using System;
using System.Windows;

namespace ELRCRobTool
{
    public static class Program
    {
        private static bool _stopCurrentAction = false;

        public static bool ShouldStop() => _stopCurrentAction;

        public static void SetStopAction(bool value)
        {
            _stopCurrentAction = value;
        }
    }
}