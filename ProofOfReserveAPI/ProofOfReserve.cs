namespace ProofOfReserveAPI
{
    public class ProofOfReserve
    {
        public record MerkleProofNode(string Hash, int Direction);
    }
}
