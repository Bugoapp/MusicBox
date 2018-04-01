using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using MusicBox.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace MusicBox.Models
{
    public class SongsDao : MainDataAccess
    {
        public const String BUCKET_NAME = "songstoragebytes";
        public SongsDao(RegionEndpoint region)
            :base(region)
        {}

        public Song GetSong(String artist, String song)
        {
            Song loSong = null;
            try
            {
                Table loTable = GetTable("Music");
                Document loDocument = loTable.GetItem(artist, song);
                if(loDocument.Count > 0)
                {
                    int liLength = loDocument["Length"].AsInt();
                    int liMinutes = liLength / 60;
                    int liSeconds = liLength % 60;
                    String lsLength = (liMinutes > 9 ? "0" : "") + Convert.ToString(liMinutes) + ":" + (liSeconds > 9 ? "0" : "")  +  Convert.ToString(liSeconds);
                    loSong = new Song()
                    {
                        Id = HashUtil.Hash(loDocument["Artist"].AsString() + loDocument["Name"].AsString()),
                        Artist = loDocument["Artist"].AsString(),
                        Name = loDocument["Name"].AsString(),
                        Length = lsLength
                    };
                }

            }
            catch (Exception)
            {

            }
            return loSong;
        }

        public List<Song> GetSongs(String search)
        {
            List<Song> loResult = new List<Song>();
            try
            {
                Table loTable = GetTable("Music");

                ScanFilter loFilter = new ScanFilter();
                if (String.IsNullOrEmpty(search))
                {
                    loFilter.AddCondition("Artist", ScanOperator.NotContains, new DynamoDBEntry[] { "-1" });
                }
                else
                {
                    loFilter.AddCondition("Artist", ScanOperator.Contains, new DynamoDBEntry[] { search.ToLower() });
                }
                
                ScanOperationConfig loConfig = new ScanOperationConfig
                {
                    AttributesToGet = new List<string> { "Artist", "Name", "Length", "File" },
                    Filter = loFilter,
                    Select = SelectValues.SpecificAttributes
                };

                Search loSearch = null;
                loSearch = loTable.Scan(loConfig);
                List<Document> loDocList = new List<Document>();

                do
                {
                    try
                    {
                        loDocList = loSearch.GetNextSet();
                    }
                    catch (Exception ex)
                    { }

                    foreach (var item in loDocList)
                    {
                        String lsArtist = item["Artist"].AsString();
                        String lsName = item["Name"].AsString();
                        int liLength = item["Length"].AsInt();
                        String lsFilename = item["File"].AsString();

                        int liMinutes = liLength / 60;
                        int liSeconds = liLength % 60;
                        String lsLength = (liMinutes > 9 ? "" : "0") + Convert.ToString(liMinutes) + ":" + (liSeconds > 9 ? "" : "0") + Convert.ToString(liSeconds);
                        Song loSong = new Song()
                        {
                            Id = HashUtil.Hash(lsArtist + lsName),
                            Artist = GetCapitalString(lsArtist),
                            Name = GetCapitalString(lsName),
                            Length = lsLength,
                            Filename = lsFilename
                        };
                        loResult.Add(loSong);
                    }

                } while (!loSearch.IsDone);

                if (!String.IsNullOrEmpty(search))
                {           
                    loFilter = new ScanFilter();
                    loFilter.AddCondition("Artist", ScanOperator.NotEqual, new DynamoDBEntry[] { "-1" });
                    loFilter.AddCondition("Name", ScanOperator.Contains, new DynamoDBEntry[] { search.ToLower() });
                    loConfig.Filter = loFilter;

                    loSearch = loTable.Scan(loConfig);
                    loDocList = new List<Document>();

                    do
                    {
                        try
                        {
                            loDocList = loSearch.GetNextSet();
                        }
                        catch (Exception ex)
                        { }

                        foreach (var item in loDocList)
                        {
                            String lsArtist = item["Artist"].AsString();
                            String lsName = item["Name"].AsString();
                            int liLength = item["Length"].AsInt();
                            String lsFilename = item["File"].AsString();

                            int liMinutes = liLength / 60;
                            int liSeconds = liLength % 60;
                            String lsLength = (liMinutes > 9 ? "" : "0") + Convert.ToString(liMinutes) + ":" + (liSeconds > 9 ? "" : "0") + Convert.ToString(liSeconds);
                            Song loSong = new Song()
                            {
                                Id = HashUtil.Hash(lsArtist + lsName),
                                Artist = GetCapitalString(lsArtist),
                                Name = GetCapitalString(lsName),
                                Length = lsLength,
                                Filename = lsFilename
                            };
                            loResult.Add(loSong);
                        }

                    } while (!loSearch.IsDone);
                }
            }
            catch (Exception)
            {

            }

            return loResult               
                .GroupBy(g => g.Id)
                .Select(s => s.First())
                .ToList();
        }

        public String GetCapitalString(String data)
        {
            String lsResult = "";
            try
            {
                lsResult = data.First().ToString().ToUpper() + data.Substring(1);
            }
            catch (Exception)
            {

            }
            return lsResult;
        }

        public byte[] GetSongBytes(String filename)
        {
            byte[] loResult = new byte[0];
            using (IAmazonS3 client = new AmazonS3Client(base.Credentials, Amazon.RegionEndpoint.USEast1))
            {
                
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = BUCKET_NAME,
                    Key = filename

                };

                using (GetObjectResponse response = client.GetObject(request))
                {
                    
                    using (Stream responseStream = response.ResponseStream)
                    {
                        loResult = new byte[responseStream.Length];
                        MemoryStream loMemory = new MemoryStream(loResult);
                        
                        byte[] loBuffer = new byte[8192];
                        int loBytesRead = 0;
                        while ((loBytesRead = responseStream.Read(loBuffer,0, loBuffer.Length)) > 0)
                        {
                            loMemory.Write(loBuffer, 0, loBytesRead);
                        }
                    }
                }             
            }
            return loResult;
        }

        public Boolean SaveSong(String path, Song song)
        {
            bool lboResult = false;
            try
            {
                if (CreateSongObject(song))
                {
                    if (UploadSongBytes(path, song.Filename))
                    {
                        lboResult = true;
                    }
                }                
            }
            catch (Exception)
            {

            }
            return lboResult;
        }

        private Boolean CreateSongObject(Song song)
        {
            bool lboResponse = false;
            try
            {
                Table loTable = GetTable("Music");
                Document loDocument = new Document();
                loDocument["Artist"] = song.Artist;
                loDocument["Name"] = song.Name;
                loDocument["Length"] = song.LengthSeconds;
                loDocument["File"] = song.Filename;
                Document loResponse = loTable.PutItem(loDocument);
                lboResponse = true;
            }
            catch (Exception)
            {

            }
            return lboResponse;
        }
        private Boolean UploadSongBytes(String path, String filename)
        {
            bool lboResult = false;
            try
            {
                using (IAmazonS3 client = new AmazonS3Client(base.Credentials, Amazon.RegionEndpoint.USEast1)) { 
                    PutObjectRequest loPutRequest = new PutObjectRequest
                    {
                        BucketName = BUCKET_NAME,
                        Key = filename,
                        FilePath = path,
                        ContentType = "audio/mp3"
                    };

                    PutObjectResponse loResponse = client.PutObject(loPutRequest);
                    if(loResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        lboResult = true;
                }
            }
            catch (Exception)
            {
            }
            return lboResult;
        }
    }
}