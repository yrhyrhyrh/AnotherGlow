import { TestBed } from '@angular/core/testing';

import { SocialActionsService } from './social-actions.service';

describe('SocialActionsService', () => {
  let service: SocialActionsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SocialActionsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
