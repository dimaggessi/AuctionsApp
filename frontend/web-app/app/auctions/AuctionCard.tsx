import Image from 'next/image'
import React from 'react'
import CountdownTimer from './CountdownTimer'
import CarImage from './CarImage'

type Props = {
    auction: any
}

// priority => attempt to load image soon as possible without lazy loading

export default function AuctionCard({auction}: Props) {
  return (
    <a href="#" className='group'>
          <div className='auction-card-container'>
            <div>
              <CarImage imageUrl={auction.imageUrl} />
              <div className='absolute bottom-2 left-2'>
                <CountdownTimer auctionEnd={auction.auctionEnd} />
              </div>
            </div>
          </div>
          <div className='flex justify-between items-center mt-4'>
            <h3 className="text-gray-700">{auction.make} {auction.model}</h3>
            <p className="font-semibold text-sm">{auction.year}</p>
          </div>
      </a>
  )
}
