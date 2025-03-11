using MissionControl.Application;
using MissionControl.Application.Interfaces;
using System;
using System.Collections.Generic;

namespace MissionControl.Infrastructure
{
    public class MissionControlsManagerService
    {
        private Dictionary<string, IMissionControlService> _missionControls = new Dictionary<string, IMissionControlService>();

        public string AddService(IMissionControlService service, string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                instanceId = Guid.NewGuid().ToString();
            }
            else if (_missionControls.ContainsKey(instanceId))
            {
                throw new ArgumentException($"Mission control instance with ID '{instanceId}' already exists.");
            }

            _missionControls.Add(instanceId, service);
            return instanceId;
        }

        public void RemoveService(string instanceId)
        {
            if (!_missionControls.ContainsKey(instanceId))
            {
                throw new KeyNotFoundException($"Mission control instance with ID '{instanceId}' not found.");
            }

            _missionControls.Remove(instanceId);
        }

        public IMissionControlService GetService(string instanceId)
        {
            if (!_missionControls.ContainsKey(instanceId))
            {
                throw new KeyNotFoundException($"Mission control instance with ID '{instanceId}' not found.");
            }

            return _missionControls[instanceId];
        }

        public bool ServiceExists(string instanceId)
        {
            return _missionControls.ContainsKey(instanceId);
        }

        public IEnumerable<string> GetAllInstanceIds()
        {
            return _missionControls.Keys;
        }
    }
}
