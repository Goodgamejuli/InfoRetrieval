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
import { PlaybarService } from '../../services/playbar.service';
import { AdminComponent } from "../admin/admin.component";
import { ArtistCardComponent } from "../../components/artist-card/artist-card.component";
import { AlbumCardComponent } from "../../components/album-card/album-card.component";
import { ArtistCardContainerComponent } from "../../components/artist-card-container/artist-card-container.component";

@Component({
  selector: 'app-home',
  imports: [TopNavComponent, SongCardComponent, CommonModule, RouterModule, HttpClientModule, AsyncPipe, PlaybarComponent, SongCardContainerComponent, AdminComponent, ArtistCardComponent, AlbumCardComponent, ArtistCardContainerComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit{
  playbarService = inject(PlaybarService);
  opensearchService = inject(OpenSearchService);

  showAdminPage: boolean = false;

  constructor(private searchBarService: SearchBarService) { }

  ngOnInit(): void {  }

  onNavigate(pageName: string) {
    if(pageName === 'search')
      this.searchBarService.isSearchVisible.next(true);
    else
    this.searchBarService.isSearchVisible.next(false);
  }

  onAdminClick() {
    this.showAdminPage = true;
  }

  onHomeClick() {
    this.showAdminPage = false;
  }
}
