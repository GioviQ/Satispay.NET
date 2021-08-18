namespace SatispayGBusiness.Models
{
    public enum Flow
    {
        MATCH_CODE,
        MATCH_USER,
        REFUND,
        PRE_AUTHORIZED
    }
    public enum PaymentType
    {
        TO_BUSINESS,
        REFUND_TO_BUSINESS
    }
    public enum Status
    {
        PENDING,
        ACCEPTED,
        CANCELED
    }
    public enum ActorType
    {
        CONSUMER,
        SHOP
    }

    public enum UpdateAction
    {
        ACCEPT,
        CANCEL,
        CANCEL_OR_REFUND
    }
}
