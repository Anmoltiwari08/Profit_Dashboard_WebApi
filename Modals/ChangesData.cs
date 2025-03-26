
namespace Bonus_Implementation_Policy_WebApi.Models
{

    public class UserTransactionInfo
    {
        // it is the total of the operation with action type 3,6
        public double Bonus { get; set; }

        public double Deposit { get; set; }

        // it is the double thriple of the deposited ammount 
        public double Max_Withdrawable_Amount { get; set; }

        // profit that is above the max amount that is debited 
        public double Extra_Profit { get; set; }

        // type 2 and profit less than 2 
        public double Withdrawn_Amount { get; set; }
    }

}
