// Copyright (c) 2009-2010 Satoshi Nakamoto
// Copyright (c) 2009-2017 The Bitcoin Core developers
// Distributed under the MIT software license, see the accompanying
// file COPYING or http://www.opensource.org/licenses/mit-license.php.

#ifndef GAINZCOIN_GAINZCOINCONSENSUS_H
#define GAINZCOIN_GAINZCOINCONSENSUS_H

#include <stdint.h>

#if defined(BUILD_GAINZCOIN_INTERNAL) && defined(HAVE_CONFIG_H)
#include <config/gainzcoin-config.h>
  #if defined(_WIN32)
    #if defined(DLL_EXPORT)
      #if defined(HAVE_FUNC_ATTRIBUTE_DLLEXPORT)
        #define EXPORT_SYMBOL __declspec(dllexport)
      #else
        #define EXPORT_SYMBOL
      #endif
    #endif
  #elif defined(HAVE_FUNC_ATTRIBUTE_VISIBILITY)
    #define EXPORT_SYMBOL __attribute__ ((visibility ("default")))
  #endif
#elif defined(MSC_VER) && !defined(STATIC_LIBGAINZCOINCONSENSUS)
  #define EXPORT_SYMBOL __declspec(dllimport)
#endif

#ifndef EXPORT_SYMBOL
  #define EXPORT_SYMBOL
#endif

#ifdef __cplusplus
extern "C" {
#endif

#define GAINZCOINCONSENSUS_API_VER 1

typedef enum gainzcoinconsensus_error_t
{
    gainzcoinconsensus_ERR_OK = 0,
    gainzcoinconsensus_ERR_TX_INDEX,
    gainzcoinconsensus_ERR_TX_SIZE_MISMATCH,
    gainzcoinconsensus_ERR_TX_DESERIALIZE,
    gainzcoinconsensus_ERR_AMOUNT_REQUIRED,
    gainzcoinconsensus_ERR_INVALID_FLAGS,
} gainzcoinconsensus_error;

/** Script verification flags */
enum
{
    gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_NONE                = 0,
    gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_P2SH                = (1U << 0), // evaluate P2SH (BIP16) subscripts
    gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_DERSIG              = (1U << 2), // enforce strict DER (BIP66) compliance
    gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_NULLDUMMY           = (1U << 4), // enforce NULLDUMMY (BIP147)
    gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_CHECKLOCKTIMEVERIFY = (1U << 9), // enable CHECKLOCKTIMEVERIFY (BIP65)
    gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_CHECKSEQUENCEVERIFY = (1U << 10), // enable CHECKSEQUENCEVERIFY (BIP112)
    gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_WITNESS             = (1U << 11), // enable WITNESS (BIP141)
    gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_ALL                 = gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_P2SH | gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_DERSIG |
                                                               gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_NULLDUMMY | gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_CHECKLOCKTIMEVERIFY |
                                                               gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_CHECKSEQUENCEVERIFY | gainzcoinconsensus_SCRIPT_FLAGS_VERIFY_WITNESS
};

/// Returns 1 if the input nIn of the serialized transaction pointed to by
/// txTo correctly spends the scriptPubKey pointed to by scriptPubKey under
/// the additional constraints specified by flags.
/// If not nullptr, err will contain an error/success code for the operation
EXPORT_SYMBOL int gainzcoinconsensus_verify_script(const unsigned char *scriptPubKey, unsigned int scriptPubKeyLen,
                                                 const unsigned char *txTo        , unsigned int txToLen,
                                                 unsigned int nIn, unsigned int flags, gainzcoinconsensus_error* err);

EXPORT_SYMBOL int gainzcoinconsensus_verify_script_with_amount(const unsigned char *scriptPubKey, unsigned int scriptPubKeyLen, int64_t amount,
                                    const unsigned char *txTo        , unsigned int txToLen,
                                    unsigned int nIn, unsigned int flags, gainzcoinconsensus_error* err);

EXPORT_SYMBOL unsigned int gainzcoinconsensus_version();

#ifdef __cplusplus
} // extern "C"
#endif

#undef EXPORT_SYMBOL

#endif // GAINZCOIN_GAINZCOINCONSENSUS_H
