using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using Trees;
using static ProofOfReserveAPI.ProofOfReserve;

namespace ProofOfReserveAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProofOfReserveController : ControllerBase
    {
        private static readonly List<(int UserId, int Balance)> userInfo = new List<(int UserId, int Balance)>
        {
            (1, 1111), (2, 2222), (3, 3333), (4, 4444),
            (5, 5555), (6, 6666), (7, 7777), (8, 8888),
        };

        private MerkleTree merkleTree = null!;
        private Dictionary<int, string> serializedMap = null!;

        private readonly ILogger<ProofOfReserveController> _logger;
        private readonly IConfiguration _configuration;

        public ProofOfReserveController(ILogger<ProofOfReserveController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            string branch = _configuration["Branch"] ?? "ProofOfReserve_Branch";
            string leaf = _configuration["Leaf"] ?? "ProofOfReserve_Leaf";

            serializedMap = userInfo.ToDictionary(
                user => user.UserId,
                user => $"({user.UserId},{user.Balance})"
            );

            merkleTree = new MerkleTree(serializedMap.Values.ToList(), leaf, branch);
        }

        [HttpGet(Name = "root")]
        public IActionResult GetMerkleRoot()
        {
            return Ok(new
            {
                MerkleRoot = merkleTree.GetMerkleRoot()
            });
        }

        [HttpGet("proof/{userId}")]
        public IActionResult GetProof(int userId)
        {
            if (!serializedMap.TryGetValue(userId, out var balance))
                return NotFound($"User ID {userId} not found.");

            var leaf = merkleTree.Leaves.FirstOrDefault(l => l.Data == balance);
            if (leaf == null)
                return NotFound($"User ID {userId} not found in Merkle Tree.");

            var proof = new List<MerkleProofNode>();
            var current = leaf;

            while (current?.Parent != null)
            {
                var sibling = current.Position == NodePos.Left ? current.Parent.Right : current.Parent.Left;
                if (sibling != null)
                {
                    proof.Add(new MerkleProofNode(sibling.Hash, sibling.Position == NodePos.Left ? 0 : 1));
                }
                current = current.Parent;
            }

            return Ok(new
            {
                UserId = userId,
                Balance = userInfo.First(u => u.UserId == userId).Balance,
                MerkleProof = proof
            });
        }
    }
}
