using LogicModels.BaseModels;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LogicModels
{

    public delegate void EventAdd(object sender, EventArgs e);
    public delegate void EventRunSimulation(object sender, EventArgs e, Flights flights);
    public delegate void EventWaiting(object sender, EventArgs e, Flights flights);
    public delegate void EventAddFromWaiting(object sender, EventArgs e);
    public delegate Task EventSendSimulatorData();
    public class Service
    {
        public Service()
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .CreateLogger();
        }

        public async void HookFunction(EventAdd addEvent) //Task adds planes to the simulator
        {
            while (true)
            {
                addEvent.Invoke(this, EventArgs.Empty);
                await Task.Delay(3000);
            }
        }

        public async void HookFunction(EventWaiting addEvent, Flights flights) //Task adds planes from waiting spot in simulator to waiting queue and back to airport
        {
            while (true)
            {
                addEvent.Invoke(this, EventArgs.Empty, flights);
                await Task.Delay(5000);
            }
        }

        public async void HookFunction(EventAddFromWaiting addEvent) //Task sends planes from waiting queue back to the simulation
        {
            while (true)
            {
                addEvent.Invoke(this, EventArgs.Empty);
                await Task.Delay(7000);
            }
        }

        public async void HookFunction(EventRunSimulation simulation, Flights flights) // Task that runs the simulation functions
        {
            while (true)
            {
                simulation.Invoke(this, EventArgs.Empty ,flights);
                await Task.Delay(2000);
            }
        }

        public async void HookFunction(EventSendSimulatorData simulation) // Task that sends simulation data
        {
            while (true)
            {
                simulation.Invoke();
                await Task.Delay(1000);
            }
        }
    }
}
