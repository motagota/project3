using System;
using UnityEngine;

namespace V2.Data
{
    public class Connector : Entity
    {
        private Entity _inputConnector;
        
        public event Action<Connector, Entity> OnConnectionChanged;
        public Connector(Vector2Int localPosition) : base(localPosition)
        {
        }

        public Vector2Int GetFrontPosition()
        {
            switch ((int)Rotation)
            {
                case 0 :
                    return LocalPostion + new Vector2Int(-1,0);
                case 90 :
                    return LocalPostion + new Vector2Int(0,-1);
                case 180 :
                    return LocalPostion + new Vector2Int(1,0);
                case 270 :
                    return LocalPostion + new Vector2Int(0,1);
                default:
                    return LocalPostion;
            }
            
        }

        public void Rotate(ChunkData chunk)
        {
            base.Rotate();
            CheckForConnection(chunk);
        }

        public bool CheckForConnection(ChunkData chunk)
        {
            Vector2Int frontPos = GetFrontPosition();
            Entity connection = chunk.GetMachineAt(frontPos);
            if (connection == null)
            {
                connection = chunk.GetBeltAt(frontPos);
            }
            if (connection != _inputConnector)
            {
                _inputConnector = connection;
                OnConnectionChanged?.Invoke(this, connection);
                return true;
            }
            
            return false;
        }


        public Entity GetConnectedMachine()
        {
            return _inputConnector;
        }
    }
}