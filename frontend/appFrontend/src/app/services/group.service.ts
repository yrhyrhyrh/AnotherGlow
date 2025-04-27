import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GroupData } from '../interfaces/groupData';
import { UserSuggestion } from '../interfaces/userSuggestion';
import { AddNewMemberRequest } from '../interfaces/addNewMemberRequest';

@Injectable({
  providedIn: 'root'
})
export class GroupService {
  private apiUrl = 'http://localhost:5181/api/group';

  constructor(private http: HttpClient) { }

  getGroupDetails(groupId: string): Observable<GroupData> {
    return this.http.get<GroupData>(`${this.apiUrl}/detail/${groupId}`);
  }

  searchUsersNotInGroup(groupId: string, keyword: string): Observable<UserSuggestion[]> {
    return this.http.get<UserSuggestion[]>(`${this.apiUrl}/getUsersToAdd/${groupId}?keyword=${keyword}`);
  }

  addMembers(request: AddNewMemberRequest): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(`${this.apiUrl}/addMembers`, request);
  }

  removeMember(groupMemberId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/removeMember?groupMemberId=${groupMemberId}`, {});
  }

  makeAdmin(groupMemberId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/makeAdmin?groupMemberId=${groupMemberId}`, {});
  }

  updateGroup(formData: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/update`, formData);
  }
}
