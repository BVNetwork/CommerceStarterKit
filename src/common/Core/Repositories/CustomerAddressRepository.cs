/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using Mediachase.BusinessFoundation.Data;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Customers;
using OxxCommerceStarterKit.Core.Objects;
using OxxCommerceStarterKit.Core.Repositories.Interfaces;

namespace OxxCommerceStarterKit.Core.Repositories
{
    public class CustomerAddressRepository : IRepository<Address>, ICustomerAddressRepository
    {
        private CustomerContact _currentContact;

        public CustomerAddressRepository(): this(CustomerContext.Current.CurrentContact)
        {
        }

        private CustomerAddressRepository(CustomerContact currentContact)
        {
            _currentContact = currentContact;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IList<Address> GetAll()
        {
            if(_currentContact == null)
                return new List<Address>();

            return _currentContact.ContactAddresses.Select(
                a =>
                    new Address()
                    {
                        FirstName = a.FirstName ?? _currentContact.FirstName,
                        LastName = a.LastName ?? _currentContact.LastName,
                        Id = a.AddressId.ToString(),
                        StreetAddress = a.Line1,
                        ZipCode = a.PostalCode,
                        City = a.City,
                        IsPreferredBillingAddress = _currentContact.PreferredBillingAddressId == a.AddressId,
                        IsPreferredShippingAddress = _currentContact.PreferredShippingAddressId == a.AddressId,
						CountryCode = a.CountryCode
                    }).ToList();


        }

        public Address Get(int id)
        {
            throw new NotImplementedException();
        }

        public Address GetDefaultShippingAddress()
        {
            return GetAll().FirstOrDefault(a => a.IsPreferredShippingAddress) ?? new Address();
        }

        public Address GetDefaultBillingAddress()
        {
            return GetAll().FirstOrDefault(a => a.IsPreferredBillingAddress) ?? new Address();
        }

        public void Save(Address model)
        {
            CustomerAddress address = _currentContact.ContactAddresses.FirstOrDefault(a => a.AddressId.ToString() == model.Id);

            PrimaryKeyId addressId;

            if (address == null)
            {
                address = CustomerAddress.CreateForApplication(Mediachase.Commerce.Core.AppContext.Current.ApplicationId);
               
                _currentContact.AddContactAddress(address);
                _currentContact.SaveChanges();
                addressId = address.AddressId;               
            }
            else
            {
                addressId = address.AddressId;
            }

			address.Name = string.Format("{0}, {1} {2}", model.StreetAddress, model.City, model.ZipCode);
            address.FirstName = model.FirstName;
            address.LastName = model.LastName;
            address.Line1 = model.StreetAddress;
            address.PostalCode = model.ZipCode;
            address.City = model.City;
			address.CountryCode = model.CountryCode;
			if (string.IsNullOrEmpty(address.Name))
			{
				address.Name = Guid.NewGuid().ToString();
			}
          
            _currentContact.SaveChanges();

            if (model.IsPreferredBillingAddress)
                _currentContact.PreferredBillingAddressId = addressId;

            if (model.IsPreferredShippingAddress)
                _currentContact.PreferredShippingAddressId = addressId;

    
            BusinessManager.Update(address);
            _currentContact.SaveChanges();
        }

		public void SetCustomer(CustomerContact contact)
		{
			_currentContact = contact;
		}

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
