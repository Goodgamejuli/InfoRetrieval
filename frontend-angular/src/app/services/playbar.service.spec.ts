import { TestBed } from '@angular/core/testing';

import { PlaybarService } from './playbar.service';

describe('PlaybarService', () => {
  let service: PlaybarService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PlaybarService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
