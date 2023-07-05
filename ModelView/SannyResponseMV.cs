﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelView
{
    public class SannyResponseMV
    {
        //Musoni Response Details From Musoni
        public class SearchResponse
        {
            public string entityId { get; set; }
            public string entityName { get; set; }
            public string entityType { get; set; }
            public string parentId { get; set; }
            public string parentName { get; set; }
            public Entity entityStatus { get; set; }
            public string groupId { get; set; }
            public string groupName { get; set; }
            public string officeId { get; set; }
            public string officeName { get; set; }
            public string parentAccountNo { get; set; }
        }

        public class Entity
        {
            public string id { get; set; }
            public string code { get; set; }
            public string value { get; set; }
        }

        // Edited Musoni Response

        public class SearchDetailsResponse
        {
            public string statusCode { get; set; }
            public string notification { get; set; }
            public DetailsHeld holder { get; set; }
        }

        public class DetailsHeld
        {
            public string entityId { get; set; }
            public string entityName { get; set; }
            public string parentId { get; set; }
            public string parentName { get; set; }
            public string parentAccountNo { get; set; }
        }


        //Musoni Response On Balance Account

        public class loanClientStatus
        {
            public string id { get; set; }
            public string code { get; set; }
            public string value { get; set; }
            public string pendingApproval { get; set; }
            public string waitingForDisbursal { get; set; }
            public bool active { get; set; }
            public string restructured { get; set; }
            public string closedObligationsMet { get; set; }
            public string closedWrittenOff { get; set; }
            public string closedRescheduled { get; set; }
            public string closed { get; set; }
            public string overpaid { get; set; }
        }

        public class loanClientloanType
        {
            public string id { get; set; }
            public string code { get; set; }
            public string value { get; set; }
        }

        public class loanClientCurrency
        {
            public string code { get; set; }
            public string name { get; set; }
            public string decimalPlaces { get; set; }
            public string inMultiplesOf { get; set; }
            public string nameCode { get; set; }
            public string displayLabel { get; set; }
        }

        public class loanClientTimeLine
        {
            public List<string> submittedOnDate { get; set; }
            public string submittedByUsername { get; set; }
            public string submittedByFirstname { get; set; }
            public string submittedByLastname { get; set; }
            public List<string> approvedOnDate { get; set; }
            public string approvedByUsername { get; set; }
            public string approvedByFirstname { get; set; }
            public string approvedByLastname { get; set; }
            public List<string> expectedDisbursementDate { get; set; }
            public List<string> actualDisbursementDate { get; set; }
            public string disbursedByUsername { get; set; }
            public string disbursedByFirstname { get; set; }
            public string disbursedByLastname { get; set; }
            public List<string> closeOnDate { get; set; }
            public List<string> expectedMaturityDate { get; set; }

        }

        public class ClientLoanBalance
        {

            public string id { get; set; }
            public string accountNo { get; set; }
            public string productId { get; set; }
            public string productName { get; set; }
            public string shortProductName { get; set; }
            public loanClientStatus status { get; set; }
            public loanClientloanType loanType { get; set; }
            public string loanCycle { get; set; }
            public loanClientTimeLine timeline { get; set; }
            public string submittedOnDate { get; set; }
            public string submittedByUsername { get; set; }
            public string submittedByFirstname { get; set; }
            public string submittedByLastname { get; set; }
            public string approvedOnDate { get; set; }
            public string approvedByUsername { get; set; }
            public string approvedByFirstname { get; set; }
            public string approvedByLastname { get; set; }
            public string expectedDisbursementDate { get; set; }
            public string actualDisbursementDate { get; set; }
            public string disbursedByUsername { get; set; }
            public string disbursedByFirstname { get; set; }
            public string disbursedByLastname { get; set; }
            public string closedOnDate { get; set; }
            public string expectedMaturityDate { get; set; }
            public bool inArrears { get; set; }
            public string originalLoan { get; set; }
            public string loanBalance { get; set; }
            public string amountPaid { get; set; }
            public loanClientCurrency currency { get; set; }
            public string loanCounter { get; set; }
            public string loanProductCounter { get; set; }
        }

        public class ClientBalanceloanAccounts
        {
            public List<ClientLoanBalance> loanAccounts { get; set; }
        }


        //Musoni Summar Balance Response

        public class ClientLoanBalanceSummary
        {
            string id { get; set; }
            string accountNo { get; set; }
            string productName { get; set; }
            decimal originalAmount { get; set; }
            decimal loanBalace { get; set; }
            decimal amountPaid { get; set; }


        }

    }

}
