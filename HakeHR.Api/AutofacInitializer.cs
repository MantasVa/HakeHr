using System.Configuration;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using HakeHR.Application.Infrastructure.Models;
using HakeHR.Application.Queries;
using HakeHR.Persistence.Data;
using HakeHR.Persistence.Repositories;
using HakeHR.Persistence.Repositories.Interfaces;
using MediatR.Extensions.Autofac.DependencyInjection;
using Serilog;


namespace HakeHR.Api
{
    /// <summary>
    /// Dependency injection initialization,creation class
    /// </summary>
    public static class AutofacInitializer
    {
        /// <summary>
        /// Method to setup dependency injection container
        /// </summary>
        public static void SetAutofacContainer()
        {
            Log.Information("Setting up dependency injection container");

            var builder = new ContainerBuilder();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterMediatR(typeof(GetContracts).Assembly);
            builder.RegisterType<EmployeeRepository>().As<IEmployeeRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ContractRepository>().As<IContractRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TeamRepository>().As<ITeamRepository>().InstancePerLifetimeScope();
            builder.RegisterType<OrganizationRepository>().As<IOrganizationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<AttachmentRepository>().As<IAttachmentRepository>().InstancePerLifetimeScope();
            builder.Register(c =>
                new ApplicationDbContext(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString));
            builder.Register(c =>
                new BlobStorageConnection(ConfigurationManager.AppSettings["STORAGEACCOUNT_CONNECTIONSTRING"],
                                          ConfigurationManager.AppSettings["BLOBSTORAGE_CONTAINERNAME"]));

            IContainer container = builder.Build();

            var resolver = new AutofacWebApiDependencyResolver(container);
            GlobalConfiguration.Configuration.DependencyResolver = resolver;

        }
    }
}