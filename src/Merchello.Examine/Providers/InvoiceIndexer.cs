﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Merchello.Core;
using Merchello.Core.Models;

namespace Merchello.Examine.Providers
{
    public class InvoiceIndexer : BaseMerchelloIndexer
    {
        protected override void PerformIndexAll(string type)
        {
            if (!SupportedTypes.Contains(type)) return;

            var invoices = DataService.InvoiceDataService.GetAll();
            var invoicesArray = invoices as IInvoice[] ?? invoices.ToArray();

            if (!invoicesArray.Any()) return;
            var nodes = invoicesArray.Select(i => i.SerializeToXml().Root).ToList();

            AddNodesToIndex(nodes, IndexTypes.Invoice);
        }

        public override void RebuildIndex()
        {
            DataService.LogService.AddVerboseLog(-1, "Rebuilding index");

            EnsureIndex(true);

            PerformIndexAll(IndexTypes.Invoice);
            //base.RebuildIndex();
        }

        /// <summary>
        /// Adds the invoice to the index
        /// </summary>
        /// <param name="invoice"></param>
        /// <remarks>For testing</remarks>
        internal void AddInvoiceToIndex(IInvoice invoice)
        {
            var nodes = new List<XElement> {invoice.SerializeToXml().Root};
            AddNodesToIndex(nodes, IndexTypes.Invoice);
        }

        /// <summary>
        /// Removes the invoice from the index
        /// </summary>
        /// <param name="invoice"></param>
        /// <remarks>For testing</remarks>
        internal void DeleteInvoiceFromIndex(IInvoice invoice)
        {            
            DeleteFromIndex(((Invoice)invoice).ExamineId.ToString(CultureInfo.InvariantCulture));
        }



        protected override IEnumerable<string> SupportedTypes
        {
            get { return new[] { IndexTypes.Invoice }; }
        }

        internal static readonly List<StaticField> IndexFieldPolicies
            = new List<StaticField>()
            {
                new StaticField("invoiceKey", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField("customerKey", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField("invoiceNumberPrefix", FieldIndexTypes.NOT_ANALYZED, true, string.Empty),
                new StaticField("invoiceNumber", FieldIndexTypes.ANALYZED, true, string.Empty),
                new StaticField("prefixedInvoiceNumber", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField("invoiceDate", FieldIndexTypes.ANALYZED, true, "DATETIME"),
                new StaticField("invoiceStatusKey", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField("versionKey", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("billToName", FieldIndexTypes.ANALYZED, true, string.Empty),
                new StaticField("billToAddress1", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("billToAddress2", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("billToLocality", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField("billToRegion", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("billToPostalCode", FieldIndexTypes.ANALYZED, true, string.Empty),
                new StaticField("billToCountryCode", FieldIndexTypes.ANALYZED, true, string.Empty),
                new StaticField("billToEmail", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField("billtoPhone", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField("billtoCompany", FieldIndexTypes.ANALYZED, true, string.Empty),
                new StaticField("exported", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("archived", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField("total", FieldIndexTypes.ANALYZED, true, "DOUBLE"),
                new StaticField("invoiceStatus", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("invoiceItems", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("createDate", FieldIndexTypes.NOT_ANALYZED, false, "DATETIME"),
                new StaticField("updateDate", FieldIndexTypes.NOT_ANALYZED, false, "DATETIME"),
                new StaticField("allDocs", FieldIndexTypes.ANALYZED, false, string.Empty)
            };


        /// <summary>
        /// Creates an IIndexCriteria object based on the indexSet passed in and our DataService
        /// </summary>
        /// <param name="indexSet"></param>
        /// <returns></returns>
        /// <remarks>
        /// If we cannot initialize we will pass back empty indexer data since we cannot read from the database
        /// </remarks>
        protected override IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
            return indexSet.ToIndexCriteria(DataService.InvoiceDataService.GetIndexFieldNames(), IndexFieldPolicies);
        }

        /// <summary>
        /// return the index policy for the field name passed in, if not found, return normal
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected override FieldIndexTypes GetPolicy(string fieldName)
        {
            var def = IndexFieldPolicies.Where(x => x.Name == fieldName).ToArray();
            return (def.Any() == false ? FieldIndexTypes.ANALYZED : def.Single().IndexType);
        }

    }
}