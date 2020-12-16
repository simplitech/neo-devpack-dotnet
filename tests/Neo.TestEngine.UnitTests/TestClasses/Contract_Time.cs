using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

namespace Neo.Compiler.MSIL.UnitTests.TestClasses
{
    public class Contract_Time : SmartContract.Framework.SmartContract
    {
        public static ulong getTime()
        {
            return Runtime.Time;
        }

        public static object getBlock(uint blockIndex)
        {
            return Blockchain.GetTransactionFromBlock(blockIndex, 0);
        }
    }
}