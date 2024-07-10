import React from 'react'
import AuctionCard from './AuctionCard';

async function getData() {
  // fetch and cache the data
  // it's happening on Server Side, 
  // no information about the GatewayService will be displayed on browser's console.
  const response = await fetch('http://localhost:6001/search?pageSize=10')

  if (!response.ok) throw new Error('Failed to fetch data');

  return response.json();
}


export default async function Listings() {
  const data = await getData();

  return (
    <div className='grid grid-cols-4 gap-6'>
      {data && data.results.map((auction: any) => (
        <AuctionCard auction={auction} key={auction.id} />
      ))}
    </div>
  )

  // return (
  //   <div>
  //      {JSON.stringify(data, null)}
  //   </div>
  // )
}
