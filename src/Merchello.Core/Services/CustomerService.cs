﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Merchello.Core.Events;
using Merchello.Core.Models;
using Merchello.Core.Persistence;
using Merchello.Core.Persistence.UnitOfWork;
using Umbraco.Core;
using Umbraco.Core.Events;

namespace Merchello.Core.Services
{
    /// <summary>
    /// Represents the Customer Service, 
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;
        private readonly RepositoryFactory _repositoryFactory;

        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public CustomerService()
            : this(new RepositoryFactory())
        { }

        public CustomerService(RepositoryFactory repositoryFactory)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        { }

        public CustomerService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
        {
            Mandate.ParameterNotNull(provider, "provider");
            Mandate.ParameterNotNull(repositoryFactory, "repositoryFactory");
            
            _uowProvider = provider;
            _repositoryFactory = repositoryFactory;
        }

        #region ICustomerService Members

        /// <summary>
        /// Crates an <see cref="IAnonymousCustomer"/> and saves it to the database
        /// </summary>
        /// <returns><see cref="IAnonymousCustomer"/></returns>
        public IAnonymousCustomer CreateAnonymousCustomerWithKey()
        {
            var anonymous = new AnonymousCustomer();
            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateAnonymousCustomerRepository(uow))
                {
                    repository.AddOrUpdate(anonymous);
                    uow.Commit();
                }
            }
            return anonymous;
        }

        /// <summary>
        /// Creates a customer without saving to the database
        /// </summary>
        /// <param name="firstName">The first name of the customer</param>
        /// <param name="lastName">The last name of the customer</param>
        /// <param name="email">the email address of the customer</param>
        /// <param name="memberId">The Umbraco member Id of the customer</param>
        /// <returns><see cref="ICustomer"/></returns>
        internal ICustomer CreateCustomer(string firstName, string lastName, string email, int? memberId = null)
        {

            var customer = new Customer(0, 0, null)
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    MemberId = memberId
                };

            Created.RaiseEvent(new Events.NewEventArgs<ICustomer>(customer), this);

            return customer;
        }

        /// <summary>
        /// Creates a customer and saves the record to the database
        /// </summary>
        /// <param name="firstName">The first name of the customer</param>
        /// <param name="lastName">The last name of the customer</param>
        /// <param name="email">the email address of the customer</param>
        /// <param name="memberId">The Umbraco member Id of the customer</param>
        /// <returns><see cref="ICustomer"/></returns>
        internal ICustomer CreateCustomerWithKey(string firstName, string lastName, string email, int? memberId = null)
        {
            var customer = new Customer(0, 0, null)
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                MemberId = memberId
            };

            if (Creating.IsRaisedEventCancelled(new Events.NewEventArgs<ICustomer>(customer), this))
            {
                customer.WasCancelled = true;
                return customer;
            }

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateCustomerRepository(uow))
                {
                    repository.AddOrUpdate(customer);
                    uow.Commit();
                }
            }

            Created.RaiseEvent(new Events.NewEventArgs<ICustomer>(customer), this);

            return customer;
        }

        /// <summary>
        /// Creates a customer with the Umbraco member id passed
        /// </summary>
        /// <param name="memberId">The Umbraco member id (int)</param>
        /// <returns><see cref="ICustomer"/></returns>
        internal ICustomer CreateCustomerWithKey(int memberId)
        {
            return CreateCustomerWithKey(string.Empty, string.Empty, string.Empty, memberId);
        }

        /// <summary>
        /// Saves a single <see cref="IAnonymousCustomer"/>
        /// </summary>
        /// <param name="anonymous">The <see cref="IAnonymousCustomer"/> to save</param>
        public void Save(IAnonymousCustomer anonymous)
        {
            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateAnonymousCustomerRepository(uow))
                {
                    repository.AddOrUpdate(anonymous);
                    uow.Commit();
                }
            }
        }

        /// <summary>yg
        /// Saves a single <see cref="ICustomer"/> object
        /// </summary>
        /// <param name="customer">The <see cref="ICustomer"/> to save</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        internal void Save(ICustomer customer, bool raiseEvents = true)
        {
            if(raiseEvents) Saving.RaiseEvent(new SaveEventArgs<ICustomer>(customer), this);

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateCustomerRepository(uow))
                {
                    repository.AddOrUpdate(customer);
                    uow.Commit();
                }                
            }

            if (raiseEvents) Saved.RaiseEvent(new SaveEventArgs<ICustomer>(customer), this);
        }

        /// <summary>
        /// Saves a collection of <see cref="ICustomer"/> objects.
        /// </summary>
        /// <param name="customers">Collection of <see cref="ICustomer"/> to save</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events</param>
        internal void Save(IEnumerable<ICustomer> customers, bool raiseEvents = true)
        {
            var customerArray = customers as ICustomer[] ?? customers.ToArray();

            if (raiseEvents) Saving.RaiseEvent(new SaveEventArgs<ICustomer>(customerArray), this);

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateCustomerRepository(uow))
                {
                    foreach (var customer in customerArray)
                    {
                        repository.AddOrUpdate(customer);
                    }
                    uow.Commit();
                }               
            }

            if (raiseEvents) Saved.RaiseEvent(new SaveEventArgs<ICustomer>(customerArray), this);
        }

        /// <summary>
        /// Deletes a single <see cref="IAnonymousCustomer"/>
        /// </summary>
        /// <param name="anonymous">The <see cref="IAnonymousCustomer"/> to delete</param>
        public void Delete(IAnonymousCustomer anonymous)
        {
            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateAnonymousCustomerRepository(uow))
                {
                    repository.Delete(anonymous);
                    uow.Commit();
                }
            }
        }        

        /// <summary>
        /// Deletes a collection of <see cref="IAnonymousCustomer"/> objects
        /// </summary>
        /// <param name="anonymouses">Collection of <see cref="IAnonymousCustomer"/> to delete</param>
        public void Delete(IEnumerable<IAnonymousCustomer> anonymouses)
        {
            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateAnonymousCustomerRepository(uow))
                {
                    foreach (var anonymous in anonymouses)
                    {
                        repository.Delete(anonymous);
                    }
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// Deletes a single <see cref="ICustomer"/> object
        /// </summary>
        /// <param name="customer">The <see cref="ICustomer"/> to delete</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events</param>
        internal void Delete(ICustomer customer, bool raiseEvents = true)
        {
            if (raiseEvents) Deleting.RaiseEvent(new DeleteEventArgs<ICustomer>(customer), this);

            using(new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateCustomerRepository(uow))
                {
                    repository.Delete(customer);
                    uow.Commit();
                }
            }
            if (raiseEvents) Deleted.RaiseEvent(new DeleteEventArgs<ICustomer>(customer), this);
        }

        /// <summary>
        /// Deletes a collection <see cref="ICustomer"/> objects
        /// </summary>
        /// <param name="customers">Collection of <see cref="ICustomer"/> to delete</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events</param>
        internal void Delete(IEnumerable<ICustomer> customers, bool raiseEvents = true)
        {
            var customerArray = customers as ICustomer[] ?? customers.ToArray();

            if (raiseEvents) Deleting.RaiseEvent(new DeleteEventArgs<ICustomer>(customerArray), this);

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateCustomerRepository(uow))
                {
                    foreach (var customer in customerArray)
                    {
                        repository.Delete(customer);
                    }
                    uow.Commit();                    
                }                
            }

            if (raiseEvents) Deleted.RaiseEvent(new DeleteEventArgs<ICustomer>(customerArray), this);
        }

        /// <summary>
        /// Gets a customer by its unique id
        /// </summary>
        /// <param name="key">Guid key for the customer</param>
        /// <returns><see cref="ICustomer"/></returns>
        internal ICustomer GetByKey(Guid key)
        {
            using (var repository = _repositoryFactory.CreateCustomerRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Get(key);
            }
        }
        
        /// <summary>
        /// Gets an <see cref="ICustomer"/> or <see cref="IAnonymousCustomer"/> object by its 'UniqueId'
        /// </summary>
        /// <param name="entityKey">Guid key of either object to retrieve</param>
        /// <returns><see cref="ICustomerBase"/></returns>
        public ICustomerBase GetAnyByKey(Guid entityKey)
        {
            ICustomerBase customer;

            // try retrieving an anonymous customer first as in most situations this will be what is being queried
            using (var repository = _repositoryFactory.CreateAnonymousCustomerRepository(_uowProvider.GetUnitOfWork()))
            {
                customer = repository.Get(entityKey);
            }

            if (customer != null) return customer;

            // try retrieving an existing customer
            using (var repository = _repositoryFactory.CreateCustomerRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetByEntityKey(entityKey);
            }
        }

        /// <summary>
        /// Gets a list of customer give a list of unique keys
        /// </summary>
        /// <param name="keys">List of unique keys</param>
        /// <returns></returns>
        internal IEnumerable<ICustomer> GetByKeys(IEnumerable<Guid> keys)
        {
            using (var repository = _repositoryFactory.CreateCustomerRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(keys.ToArray());
            }
        }

        /// <summary>
        /// Gets an <see cref="ICustomer"/> object by its Umbraco MemberId
        /// </summary>
        /// <param name="memberId">The Umbraco MemberId of the customer to return</param>
        /// <returns><see cref="ICustomer"/> object or null if not found</returns>
        public ICustomer GetByMemberId(int? memberId)
        {
            using (var repository = _repositoryFactory.CreateCustomerRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetByMemberId(memberId);
            }
        }

        #endregion

        /// <summary>
        /// For testing
        /// </summary>
        internal IEnumerable<IAnonymousCustomer> GetAllAnonymousCustomers()
        {
            using (var repository = _repositoryFactory.CreateAnonymousCustomerRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetAll();
            }
        }


        internal IEnumerable<ICustomer> GetAll()
        {
            using (var repository = _repositoryFactory.CreateCustomerRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetAll();
            }
        }


        #region Event Handlers


        /// <summary>
        /// Occurs before Create
        /// </summary>
        public static event TypedEventHandler<ICustomerService, Events.NewEventArgs<ICustomer>> Creating;

        /// <summary>
        /// Occurs after Create
        /// </summary>
        public static event TypedEventHandler<ICustomerService, Events.NewEventArgs<ICustomer>> Created;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<ICustomerService, SaveEventArgs<ICustomer>> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<ICustomerService, SaveEventArgs<ICustomer>> Saved;

        /// <summary>
        /// Occurs before Delete
        /// </summary>		
        public static event TypedEventHandler<ICustomerService, DeleteEventArgs<ICustomer>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<ICustomerService, DeleteEventArgs<ICustomer>> Deleted;



        #endregion

    }
}