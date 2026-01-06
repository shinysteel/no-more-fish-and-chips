using PurrNet;
using UnityEngine;
using FishFlingers.Environments;

namespace FishFlingers.Entities
{
    public class Item : NetEntity
    {
        //protected override void OnSpawned()
        //{
        //    int minSpread = 3;
        //    int x = Random.Range(Mathf.Min(-minSpread, _raft.LeftmostColumn), Mathf.Max(minSpread, _raft.RightmostColumn));

        //    int forwardDist = 10;
        //    int y = _raft.ForwardmostRow + forwardDist;

        //    transform.position = _raft.CellToWorldPosition(new Vector2Int(x, y));
        //}

        //private void Update()
        //{
        //    DriftUpdate();
        //}

        //private void DriftUpdate()
        //{

        //}
    }
}