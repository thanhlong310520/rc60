using Raccoon.EnumHolder;
using UnityEngine;
namespace Raccoon.Controller
{

    public interface InteractObjectByTrigger
    {
        void HandleStartInteract(Transform contactor);
        void HandleStopInteract(Transform contactor);
        void Raise();
    }

    public interface IReceive
    {
        void KnockBack(int value, Transform sender);
    }

    public interface IIteractSlot
    {
        void Income(long income);  
    }

    public interface ICharactor
    {
        void SetStartPoint(Transform startPoint);
        
        void ContactCheckPoint();
    }
}