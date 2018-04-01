using Amazon;
using Amazon.S3;
using MusicBox.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MusicBox.Controllers
{
    public class PlayerController : Controller
    {
        // GET: Player
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PlayList()
        {
            return PartialView();
        }

        public ActionResult Upload()
        {
            return PartialView();
        }

        public ActionResult Songs( DatatableParams parameters)
        {
            SongsDao loSongDao = new SongsDao(RegionEndpoint.SAEast1);
            
            List<Song> loSongs = loSongDao.GetSongs(parameters.Search.Value);

            if(parameters.Order.Length > 0)
            {
                int liColumnIndex = parameters.Order[0].Column;
                DTOrderDir loDirection = parameters.Order[0].Dir;

                if(liColumnIndex == 1)
                {
                    loSongs = (loDirection == DTOrderDir.ASC ? loSongs.OrderBy(o => o.Name) : loSongs.OrderByDescending(o => o.Name)).ToList();
                }
                else if (liColumnIndex == 2)
                {
                    loSongs = (loDirection == DTOrderDir.ASC ? loSongs.OrderBy(o => o.Artist) : loSongs.OrderByDescending(o => o.Artist)).ToList();
                    
                }
                else if (liColumnIndex == 3)
                {
                    loSongs = (loDirection == DTOrderDir.ASC ? loSongs.OrderBy(o => o.Length) : loSongs.OrderByDescending(o => o.Length)).ToList();
                }

            }
       
            DatatableResult<Song> result = new DatatableResult<Song>()
            {
                data = loSongs,
                draw = parameters.Draw,
                recordsFiltered = 2,
                recordsTotal = loSongs.Count
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public FileStreamResult StreamUploadedSongs(int? id, String file)
        {
            SongsDao loSongDao = new SongsDao(RegionEndpoint.SAEast1);
            var song = new byte[0];
            song = loSongDao.GetSongBytes(file);

            long fSize = song.Length;
            long startbyte = 0;
            long endbyte = fSize - 1;
            int statusCode = 200;
            if ((Request.Headers["Range"] != null))
            {
                string[] range = Request.Headers["Range"].Split(new char[] { '=', '-' });
                startbyte = Convert.ToInt64(range[1]);
                if (range.Length > 2 && range[2] != "")
                {
                    endbyte = Convert.ToInt64(range[2]);
                }

                if (startbyte != 0 || endbyte != fSize - 1 || range.Length > 2 && range[2] == "")
                {
                    statusCode = 206;
                }                                    
            }
            long desSize = endbyte - startbyte + 1;

            Response.StatusCode = statusCode;

            Response.ContentType = "audio/mp3";
            Response.AddHeader("Content-Accept", Response.ContentType);
            Response.AddHeader("Content-Length", desSize.ToString());
            Response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", startbyte, endbyte, fSize));

            var stream = new MemoryStream(song, (int)startbyte, (int)desSize);

            return new FileStreamResult(stream, Response.ContentType);
        }

        public ActionResult UploadFiles()
        {
            string FileName = "";
            try
            {
                HttpFileCollectionBase loFiles = Request.Files;
                for (int i = 0; i < loFiles.Count; i++)
                {
 
                    HttpPostedFileBase file = loFiles[i];
                    string loFilename;

                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] loTestfiles = file.FileName.Split(new char[] { '\\' });
                        loFilename = loTestfiles[loTestfiles.Length - 1];
                    }
                    else
                    {
                        loFilename = file.FileName;
                        FileName = file.FileName;
                    }

                    Guid loGuid= Guid.NewGuid();
                    String lsFileGUIName = loGuid.ToString().ToLower() + ".mp3";

                    String loFullFilename = Path.Combine(Server.MapPath("~/Uploads/"), lsFileGUIName);
                    file.SaveAs(loFullFilename);
                    Song loSong = GetTag(Server.MapPath("~/Uploads/"), loFilename, lsFileGUIName);

                    SongsDao loSongDao = new SongsDao(RegionEndpoint.SAEast1);
                    loSongDao.SaveSong(loFullFilename, loSong);

                    if (loSong != null)
                    {
                        System.IO.File.Delete(loFullFilename);
                    }
                }
            }
            catch (Exception)
            {

            }          
            return Json(FileName, JsonRequestBehavior.AllowGet);
        }

        private Song GetTag(String path, String filename, String guidFilename)
        {
            Song loResult = null;
            try
            {
                String loFullFilename = Path.Combine(Server.MapPath("~/Uploads/"), guidFilename);
                TagLib.File loFile = TagLib.File.Create(loFullFilename);

                if(loFile != null)
                {
                    loResult = new Song();
                    if (loFile.Tag.AlbumArtists.Length > 0)
                    {
                        loResult.Artist = loFile.Tag.AlbumArtists[0].ToLower();
                    }
                    else if(!String.IsNullOrEmpty(loFile.Tag.FirstAlbumArtist))
                    {
                        loResult.Artist = loFile.Tag.FirstAlbumArtist.ToLower();
                    }
                    else if (!String.IsNullOrEmpty(loFile.Tag.FirstPerformer))
                    {
                        loResult.Artist = loFile.Tag.FirstPerformer.ToLower();
                    }
                    else
                    {
                        loResult.Artist = "Unknown".ToLower();
                    }

                    if (!String.IsNullOrEmpty(loFile.Tag.Title))
                    {
                        loResult.Name = loFile.Tag.Title.ToLower();
                    }
                    else
                    {
                        loResult.Name = filename.Replace(".mp3", "").ToLower();
                        
                    }
                    loResult.Filename = guidFilename;
                    loResult.LengthSeconds = (long)loFile.Properties.Duration.TotalSeconds;
                }
               

            }
            catch (Exception)
            {

            }
            return loResult;
        }
    }
}