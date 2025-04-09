import React from 'react';
import { useWeb3React } from '@web3-react/core';
import { InjectedConnector } from '@web3-react/injected-connector';

// Initialize injected connector (MetaMask)
const injected = new InjectedConnector({
  supportedChainIds: [1, 3, 4, 5, 42, 56, 97, 137, 80001, 43114, 43113]
});

const ConnectWallet = ({ onConnect }) => {
  const { activate, account } = useWeb3React();

  const handleConnect = async () => {
    try {
      await activate(injected);
      if (account) {
        onConnect(account);
      }
    } catch (error) {
      console.error('Failed to connect wallet:', error);
    }
  };

  return (
    <div className="connect-wallet card">
      <h2>Welcome to DeFi Portfolio Manager</h2>
      <p>Connect your wallet to view your DeFi portfolio across multiple chains</p>
      <button className="connect-btn" onClick={handleConnect}>
        Connect Wallet
      </button>
    </div>
  );
};

export default ConnectWallet; 