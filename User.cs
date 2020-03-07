using System;

namespace KomitWap
{
    public class User
    {
        public string UserId {get; set;}
        public string TenantId {get; set;}
        public string VenueId {get; set;}
        public string HotspotId {get; set;}
        public string Event {get; set;}
        public int Timestamp {get; set;}
        public string Username {get; set;}
        public string FirstName {get; set;}
        public string LastName {get; set;}
        public string Gender {get; set;}
        public string Phone {get; set;}
        public string Email {get; set;}
        public string BirthDate {get; set;}
        public string Provider {get; set;}
        public bool Marketing {get; set;}
        public string Type {get; set;}
        public bool Online {get; set;}
        public string VisitCount {get; set;}
        public bool LockedRegistration {get; set;}
        public bool Deleted {get; set;}
        public string CivilStatus {get; set;}
        public string Country {get; set;}
        public CustomOptin CustomOptin {get; set;}
    }

    public class CustomOptin
    {
        public bool MyCustomOptInClause_1 {get; set;}
        public bool MyCustomOptInClause_2 {get; set;}
    }
}