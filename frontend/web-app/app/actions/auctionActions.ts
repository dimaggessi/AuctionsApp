'use server'

import { Auction, PagedResult } from "@/types";

export async function getData(query: string): Promise<PagedResult<Auction>> {
    // fetch and cache the data
    // it's happening on Server Side, 
    // no information about the GatewayService will be displayed on browser's console.
    const response = await fetch(`http://localhost:6001/search${query}`)
  
    if (!response.ok) throw new Error('Failed to fetch data');
  
    return response.json();
  }