using LionStudios.Suite.Analytics;
using LionStudios.Suite.Analytics.Events;
using Services.Analytics.Data.Args.BaseReflection;

namespace Services.Analytics.Data.Args.Economy
{
    public class InAppPurchaseEventArgs : ReflectionEventBase
    {
        public string PurchaseName { get; private set; }
        public Product SpentProducts { get; private set; }
        public Product ReceivedProducts { get; private set; }
        public Transaction Transaction { get; private set; }
        public int VirtualCurrencyAmount { get; private set; }
        public string VirtualCurrencyName { get; private set; }
        public string VirtualCurrencyType { get; private set; }
        public string RealCurrencyType { get; private set; }
        public float RealCurrencyAmount { get; private set; }
        public string PurchaseLocation { get; private set; }
        public string ProductID { get; private set; } = null;
        public string TransactionID { get; private set; } = null;
        public ReceiptStatus ReceiptStatus { get; private set; }

        public InAppPurchaseEventArgs(string purchaseName, Product spentProducts, Product receivedProducts,
            string purchaseLocation = "General", string productID = null, string transactionID = null,
            ReceiptStatus receiptStatus = ReceiptStatus.NoValidation)
        {
            PurchaseName = purchaseName;
            SpentProducts = spentProducts;
            ReceivedProducts = receivedProducts;
            PurchaseLocation = purchaseLocation;
            ProductID = productID;
            TransactionID = transactionID;
            ReceiptStatus = receiptStatus;
        }

        public InAppPurchaseEventArgs(Transaction transaction, string productID = null, string transactionID = null,
            string purchaseLocation = "General",
            ReceiptStatus receiptStatus = ReceiptStatus.NoValidation)
        {
            Transaction = transaction;
            ProductID = productID;
            TransactionID = transactionID;
            PurchaseLocation = purchaseLocation;
            ReceiptStatus = receiptStatus;
        }

        public InAppPurchaseEventArgs(int virtualCurrencyAmount, string virtualCurrencyName, string virtualCurrencyType,
            string realCurrencyType, float realCurrencyAmount, string purchaseName, string productID = null,
            string transactionID = null, string purchaseLocation = "General",
            ReceiptStatus receiptStatus = ReceiptStatus.NoValidation)
        {
            VirtualCurrencyAmount = virtualCurrencyAmount;
            VirtualCurrencyName = virtualCurrencyName;
            VirtualCurrencyType = virtualCurrencyType;
            RealCurrencyType = realCurrencyType;
            RealCurrencyAmount = realCurrencyAmount;
            PurchaseName = purchaseName;
            ProductID = productID;
            TransactionID = transactionID;
            PurchaseLocation = purchaseLocation;
            ReceiptStatus = receiptStatus;
        }
    }
}