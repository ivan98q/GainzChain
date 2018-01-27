// Copyright (c) 2009-2010 Satoshi Nakamoto
// Copyright (c) 2009-2017 The Bitcoin Core developers
// Distributed under the MIT software license, see the accompanying
// file COPYING or http://www.opensource.org/licenses/mit-license.php.

#include <pow.h>

#include <arith_uint256.h>
#include <chain.h>
#include <primitives/block.h>
#include <random.h>
#include <uint256.h>

int64_t GetNextPathLength(const CBlockIndex* pindexLast, const CBlockHeader *pblock, const Consensus::Params& params)
{
    assert(pindexLast != nullptr);
    uint32_t nProofOfWorkLimit = params.powMinLimit;

    // Only change once per difficulty adjustment interval
    if ((pindexLast->nHeight+1) % params.DifficultyAdjustmentInterval() != 0)
    {
        if (params.fPowAllowMinDifficultyBlocks)
        {
            // Special difficulty rule for testnet:
            // If the new block's timestamp is more than 2* 10 minutes
            // then allow mining of a min-difficulty block.
            if (pblock->GetBlockTime() > pindexLast->GetBlockTime() + params.nPowTargetSpacing*2)
                return nProofOfWorkLimit;
            else
            {
                // Return the last non-special-min-difficulty-rules-block
                const CBlockIndex* pindex = pindexLast;
                while (pindex->pprev && pindex->nHeight % params.DifficultyAdjustmentInterval() != 0 && pindex->nBits == nProofOfWorkLimit)
                    pindex = pindex->pprev;
                return pindex->nBits;
            }
        }
        return pindexLast->nBits;
    }

    // Go back by what we want to be 14 days worth of blocks
    int nHeightFirst = pindexLast->nHeight - (params.DifficultyAdjustmentInterval()-1);
    assert(nHeightFirst >= 0);
    const CBlockIndex* pindexFirst = pindexLast->GetAncestor(nHeightFirst);
    assert(pindexFirst);

    return CalculateNextPathLengthRequired(pindexLast, pindexFirst->GetBlockTime(), params);
}

int64_t CalculateNextPathLengthRequired(const CBlockIndex* pindexLast, int64_t nFirstBlockTime, const Consensus::Params& params)
{
    if (params.fPowNoRetargeting)
        return pindexLast->nBits;

    // Limit adjustment step
    int64_t nActualTimespan = pindexLast->GetBlockTime() - nFirstBlockTime;
    if (nActualTimespan < params.nPowTargetTimespan/4)
        nActualTimespan = params.nPowTargetTimespan/4;
    if (nActualTimespan > params.nPowTargetTimespan*4)
        nActualTimespan = params.nPowTargetTimespan*4;

    // Retarget
    int64_t bnNew;
    bnNew = (int64_t)pindexLast->nBits;
    bnNew *= nActualTimespan;
    bnNew /= params.nPowTargetTimespan;


    if (bnNew > params.powMaxLimit)
        bnNew = params.powMaxLimit;

    return bnNew;
}

bool CheckProofOfWork(std::vector<std::pair<std::vector<uint8_t>, uint32_t>> path, int64_t target_path_length, std::vector<std::vector<std::vector<bool>>> world, const Consensus::Params& params)
{
    int64_t bnTarget = target_path_length;

    // Check range
    if (bnTarget == 0 || bnTarget > params.powMaxLimit || bnTarget < params.powMinLimit)
        return false;

    // Check that the path length matches the claimed length
    if (path.size() > target_path_length)
        return false;

    // Follow the path

    uint32_t last_timestamp = 0;
    bool first = true;
    std::vector<uint8_t> last_pos = {0,0,0}; // x, y, z


    // This for loop just checks to see if the path is consistent
    for (const auto& tile : path) {
        std::vector<uint8_t> coordinates = tile.first;
        uint32_t timestamp = tile.second;

        if (first) {
          // We don't need to verify the first coordinate
            last_pos = coordinates;
            last_timestamp = timestamp;
            first = false;
            continue;
        }

        // Checks to make sure we didn't make some weird telport move / diagonal move.
        if (last_pos[0] + 1 != coordinates[0] || last_pos[0] -1 != coordinates[0] ||
          last_pos[1] + 1 != coordinates[1] || last_pos[1] - 1 != coordinates[1] ||
        last_pos[2] + 1 != coordinates[2] || last_pos[2] - 1 != coordinates[2]) {
          return false;
        }

        /*
        * Checks if it takes a second or more for the next move operation to take place.
        * This may have to change! Maybe just make sure that the timestamp is always positive.
        * Would enforce people mining faster (increasing the "hashes"). I know we are worried about
        * automation but that can be changed in Gainzcoin Core release v0.2 :P
        */
        if(last_timestamp + 1 > timestamp)
          return false;

        // Now we move on
        last_pos = coordinates;
        last_timestamp = timestamp;
    }

    // This is to make sure we actually found a "gold tile"
    if(world.at(last_pos[0]).at(last_pos[1]).at(last_pos[2]) != true)
      return false;

    return true;
}

std::vector<std::vector<std::vector<bool>>> GenerateWorld(uint256 prevBlockHash)
{
    std::vector<std::vector<std::vector<bool>>> world;

    FastRandomContext rand(prevBlockHash);
    for (unsigned int x = 0; x < world.size(); ++x) {
        for (unsigned int y = 0; y < world[x].size(); ++y) {
            for (unsigned int z = 0; z < world[x][y].size(); ++z) {
                if (z < 4) {
                    continue;
                }
                uint64_t res = rand.randrange(10);
                if (res == 0) {
                    world[x][y][z] = true;
                } else {
                    world[x][y][z] = false;
                }
            }
        }
    }

    return world;
}
