// Copyright (c) 2009-2010 Satoshi Nakamoto
// Copyright (c) 2009-2017 The Bitcoin Core developers
// Distributed under the MIT software license, see the accompanying
// file COPYING or http://www.opensource.org/licenses/mit-license.php.

#ifndef GAINZCOIN_POW_H
#define GAINZCOIN_POW_H

#include <consensus/params.h>

#include <stdint.h>

class CBlockHeader;
class CBlockIndex;
class uint256;

int64_t GetNextPathLength(const CBlockIndex* pindexLast, const CBlockHeader *pblock, const Consensus::Params& params);
int64_t CalculateNextPathLengthRequired(const CBlockIndex* pindexLast, int64_t nFirstBlockTime, const Consensus::Params& params);


/** Check whether a block hash satisfies the proof-of-work requirement specified by nBits */
bool CheckProofOfWork(std::vector<std::pair<std::vector<uint8_t>, uint32_t>> path, int64_t target_path_length, std::vector<std::vector<std::vector<int>>> world, const Consensus::Params& params);

#endif // GAINZCOIN_POW_H
