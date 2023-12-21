using System;

namespace CodeChallenge.Models
{
    public class Compensation
    {
        // I will not do this now, but I personally would want to create a parent class for all database objects to automatically set some columns
        // For example, a default Id, a column to track the date the row was inserted, and a column to track when the row was updated last at the very least
        // Instead, I will just create an id specifically in this Compensation class
        public string CompensationId { get; set; }

        // I know the directions said to call it Employee, but since I don't like storing an Employee object anyway, I want to call it EmployeeId
        // You could create a DTO class just to send the Employee back in the get request, but I am not going to here
        // You can just use the Employee controller if you want employee specific information after getting the compensation
        public string EmployeeId { get; set; }

        // I know there are discussions/arguments that can be had over this
        // I have heard of the decimal type and understand it should be used when doing monetary calculations, but it's a bigger object
        // We are not doing any calculation on this right now, so I am just going to use double
        // In reality, I'd just use whatever type other money-related fields used in the codebase I am looking at for consistency
        public double Salary { get; set; }

        // This one could also be the topic of discussion
        // Will it always be set on the api request? Can we technically have it active before making the request?
        // Should it be UTC, EST, whatever the system uses?
        // For this, I will use the assumption that the api call will set this field
        public DateTime EffectiveDate { get; set; }

        // other things to think about
        // what about the types of compensation?
        // would we want this to be an abstract class with sub-classes for the types of comp?
        // would we want them to share a table using a discriminator or separate tables?
        // would we want to be able to set them as active/inactive? same thing with the Employee table
        // also maybe if we are extending, it would be best for the abstract class to not have Salary, and instead call it Value or something like that
    }
}
