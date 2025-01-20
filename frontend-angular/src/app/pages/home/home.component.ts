import { Component, inject, OnInit } from '@angular/core';
import { AsyncPipe, CommonModule } from '@angular/common';
import { TopNavComponent } from "../../components/top-nav/top-nav.component";
import { SongCardComponent } from "../../components/song-card/song-card.component";
import { SearchBarService } from '../../services/searchbar.service';
import { Router, RouterModule } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { OpenSearchService } from '../../services/opensearch.service';
import { SongDTO } from '../../models/songDto';
import { PlaybarComponent } from "../../components/playbar/playbar.component";
import { SongCardContainerComponent } from "../../components/song-card-container/song-card-container.component";

@Component({
  selector: 'app-home',
  imports: [TopNavComponent, SongCardComponent, CommonModule, RouterModule, HttpClientModule, AsyncPipe, PlaybarComponent, SongCardContainerComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit{

  opensearchService = inject(OpenSearchService)

public songCards= [
    {
      song_id: 1,
      thumbnail: 'assets/song_placeholder.png',
      title: 'Test song',
      description: 'Test einer song description oder so',
      song_link: 'assets/songs/test_song.mp3'
    },
    {
      song_id: 1,
      thumbnail: 'assets/song_placeholder.png',
      title: 'Test song',
      description: 'Test einer song description oder so',
      song_link: ''
    }
  ];

  constructor(private searchBarService: SearchBarService) { }

  ngOnInit(): void {  }

  onNavigate(pageName: string) {
    if(pageName === 'search')
      this.searchBarService.isSearchVisible.next(true);
    else
    this.searchBarService.isSearchVisible.next(false);
  }

  /*
  onTestClick() {
    this.opensearchService.getSongTest();
  }*/
}
