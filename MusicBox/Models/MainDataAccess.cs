using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace MusicBox.Models
{
    public class MainDataAccess
    {
        public string AccessKey = WebConfigurationManager.AppSettings["AWSAccessKey"];
        public string SecretKey = WebConfigurationManager.AppSettings["AWSSecretKey"];
        public BasicAWSCredentials Credentials { get; set; }
        public RegionEndpoint Region { get; set; }

        public MainDataAccess(RegionEndpoint region)
        {
            this.Credentials = new BasicAWSCredentials(AccessKey, SecretKey);
            this.Region = region;
        }

        public AmazonDynamoDBClient GetClient()
        {
            AmazonDynamoDBClient loResult = null;
            try
            {
                loResult = new AmazonDynamoDBClient(this.Credentials, Region);
            }
            catch (Exception)
            {
                throw;
            }
            return loResult;
        }

        public Table GetTable(String name)
        {
            Table loResult = null;
            try
            {
                loResult = Table.LoadTable(GetClient(), name);
            }
            catch (Exception)
            {
                throw;
            }
            return loResult;
        } 
    }
}