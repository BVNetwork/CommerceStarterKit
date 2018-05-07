/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Linq;
using OxxCommerceStarterKit.Core.Repositories;

namespace OxxCommerceStarterKit.Core.Objects
{
    public class PersonalSettingsForm
    {
        public PersonalSettingsForm()
        {
            ContactRepository contactRepository = new ContactRepository();
            ContactInformation = contactRepository.Get();

            CustomerAddressRepository addressRepository = new CustomerAddressRepository();
            ShippingAddress = addressRepository.GetAll().FirstOrDefault(a => a.IsPreferredShippingAddress);            
            BillingAddress = addressRepository.GetAll().FirstOrDefault(a => a.IsPreferredBillingAddress);

            SetDefaultName(ShippingAddress, ContactInformation);
            SetDefaultName(BillingAddress, ContactInformation);

        }

        private void SetDefaultName(Address address, ContactInformation contactInformation)
        {
            if(address == null)
                return;

            if (string.IsNullOrEmpty(address.FirstName))
                address.FirstName = contactInformation.FirstName;

            if (string.IsNullOrEmpty(address.LastName))
                address.LastName = contactInformation.LastName;

        }

        public Address BillingAddress { get; set; }
        
        public Address ShippingAddress { get; set; }
             
        public ContactInformation ContactInformation { get; set; }

        public bool ConsentEmail { get; set; } 
 
        public bool ConsentSms { get; set; } 
    }
}
