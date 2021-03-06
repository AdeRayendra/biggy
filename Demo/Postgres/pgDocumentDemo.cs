﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Demo.Models;

namespace Demo {
  public class pgDocumentDemo {
    private PgChinookDb _testDb;
    private PgChinookDb _chinookDb;

    public void Run() {
      Console.WriteLine("Postgres Document Demo - TEST DATA");
      Console.WriteLine("====================================");
      Console.WriteLine("Initialize Test Db");

      var sw = new Stopwatch();

      sw.Start();
      _testDb = new PgChinookDb("biggy_test", dropCreateTables: true);
      sw.Stop();
      Console.WriteLine("Initialized and reset PG database in {0} MS", sw.ElapsedMilliseconds);

      Console.WriteLine("Write some complex artist documents, with nested albums and tracks, from sample data...");
      int qtyArtists = 1000;
      int qtyAlbumsPerArtist = 5;
      int qtyTracksPerAlbum = 8;
      var sampleArtistDocuments = SampleData.GetSampleArtistDocuments(qtyArtists, qtyAlbumsPerArtist, qtyTracksPerAlbum);

      sw.Reset();
      sw.Start();
      _testDb.ArtistDocuments.Add(sampleArtistDocuments);
      sw.Stop();
      Console.WriteLine("Wrote {0} artist document records in {1} ms", sampleArtistDocuments.Count, sw.ElapsedMilliseconds);
      Console.WriteLine("Total of {0} artists, {1} albums, and {2} tracks nested in artist documents",
                qtyArtists, qtyArtists * qtyAlbumsPerArtist, qtyArtists * qtyAlbumsPerArtist * qtyTracksPerAlbum);

      Console.WriteLine("");
      Console.WriteLine("Re-Initialize Db and read all that data from back-end...");

      sw.Reset();
      sw.Start();
      _testDb.LoadData();
      sw.Stop();
      Console.WriteLine("Read all data from store in {0} ms", sw.ElapsedMilliseconds);
      Console.WriteLine("{0} complex artist documents", _testDb.ArtistDocuments.Count);

      foreach (var artist in sampleArtistDocuments) {
        artist.Name = "UPDATED " + artist.ArtistDocumentId;
        artist.Albums.FirstOrDefault().Title = "UPDATED ALBUM";
      }

      sw.Reset();
      sw.Start();
      _testDb.ArtistDocuments.Update(sampleArtistDocuments);
      sw.Stop();
      Console.WriteLine("Updated {0} artist documents in {1} ms", sampleArtistDocuments.Count, sw.ElapsedMilliseconds);

      sw.Reset();
      sw.Start();
      _testDb.ArtistDocuments.Remove(sampleArtistDocuments);
      sw.Stop();
      Console.WriteLine("Removed {0} artist documents in {1} ms", sampleArtistDocuments.Count, sw.ElapsedMilliseconds);

      Console.WriteLine("");
      Console.WriteLine("Postgres Document Demo - CHINOOK DATA");
      Console.WriteLine("=======================================");

      Console.WriteLine("Now let's use some actual data from Chinook Db to build some documents...");

      sw.Reset();
      sw.Start();
      _chinookDb = new PgChinookDb("chinook-pg");
      sw.Stop();
      Console.WriteLine("Initialized Chinook data in {0} ms - loaded:", sw.ElapsedMilliseconds);
      Console.WriteLine("{0} Artists", _chinookDb.Artists.Count);
      Console.WriteLine("{0} Albums", _chinookDb.Albums.Count);
      Console.WriteLine("{0} Tracks", _chinookDb.Tracks.Count);

      Console.WriteLine("");
      Console.WriteLine("Nest chinook albums and tracks under artists complex documents and save");
      Console.WriteLine("=======================================================================");

      Console.WriteLine("");
      Console.WriteLine("Clear previous data from artist documents table...");
      int qtyRecordsToDelete = _testDb.ArtistDocuments.Count;

      sw.Start();
      _testDb.ArtistDocuments.Clear();
      sw.Stop();
      Console.WriteLine("tDeleted {0} artist document records in {1} ms", qtyRecordsToDelete, sw.ElapsedMilliseconds);

      Console.WriteLine("");
      Console.WriteLine("Create artist documents using chinook data...");

      sw.Reset();
      sw.Start();
      var artistDocumentsToAdd = new List<ArtistDocument>();

      // Read from Chinook db, and combine artist, album, and track info into documents:
      foreach (var artist in _chinookDb.Artists) {
        var newArtistDoc = new ArtistDocument { ArtistDocumentId = artist.ArtistId, Name = artist.Name };
        var artistAlbums = (from a in _chinookDb.Albums where a.ArtistId == artist.ArtistId select a).ToList();
        foreach (var album in artistAlbums) {
          var newAlbumDocument = new AlbumDocument { AlbumId = album.AlbumId, ArtistId = album.ArtistId, Title = album.Title };
          var albumTracks = from t in _chinookDb.Tracks where t.AlbumId == album.AlbumId select t;
          foreach (var track in albumTracks) {
            newAlbumDocument.Tracks.Add(new Track { TrackId = track.TrackId, AlbumId = newAlbumDocument.AlbumId, Name = track.Name });
          }
          newArtistDoc.Albums.Add(newAlbumDocument);
        }
        artistDocumentsToAdd.Add(newArtistDoc);
      }
      sw.Stop();
      Console.WriteLine("");
      Console.WriteLine("Read \r\n{0} artists, \r\n{1} albums, and \r\n{2} tracks \r\nfrom Chinook and assembled into complex artist documents in {3} ms",
        _chinookDb.Artists.Count, _chinookDb.Albums.Count, _chinookDb.Tracks.Count, sw.ElapsedMilliseconds);

      sw.Reset();
      sw.Start();
      _testDb.ArtistDocuments.Add(artistDocumentsToAdd);
      sw.Stop();
      Console.WriteLine("");
      Console.WriteLine("Wrote all {0} complex artist documents to PG Store in {1} ms", _testDb.ArtistDocuments.Count, sw.ElapsedMilliseconds);
    }
  }
}