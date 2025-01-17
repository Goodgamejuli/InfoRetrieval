import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { SearchBarService } from '../../services/searchbar.service';
import { CommonModule } from '@angular/common';
import { OpenSearchService } from '../../services/opensearch.service';

@Component({
  selector: 'app-top-nav',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './top-nav.component.html',
  styleUrls: ['./top-nav.component.css']
})
export class TopNavComponent {
  opensearchService = inject(OpenSearchService);
  public isSearchFieldVisible: boolean = false;

  constructor(
    private router: Router,
    private searchBarService: SearchBarService
  ) {}

  onNavigateToLogin() {
    this.router.navigate(['/', 'login']);
  }

  onTestClick() {
    this.opensearchService.getSongTest();
  }
}
