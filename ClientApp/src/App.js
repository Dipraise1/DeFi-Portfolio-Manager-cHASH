import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Web3ReactProvider } from '@web3-react/core';
import { ethers } from 'ethers';
import Header from './components/Header';
import Dashboard from './components/Dashboard';
import TokenBalances from './components/TokenBalances';
import YieldPositions from './components/YieldPositions';
import ConnectWallet from './components/ConnectWallet';
import './App.css';

// Function to get the Ethereum provider library
function getLibrary(provider) {
  const library = new ethers.providers.Web3Provider(provider);
  library.pollingInterval = 12000;
  return library;
}

function App() {
  const [walletConnected, setWalletConnected] = useState(false);
  const [walletAddress, setWalletAddress] = useState('');

  const handleWalletConnect = (address) => {
    setWalletAddress(address);
    setWalletConnected(true);
  };

  const handleWalletDisconnect = () => {
    setWalletAddress('');
    setWalletConnected(false);
  };

  return (
    <Web3ReactProvider getLibrary={getLibrary}>
      <Router>
        <div className="app">
          <Header 
            walletConnected={walletConnected} 
            walletAddress={walletAddress} 
            onDisconnect={handleWalletDisconnect} 
          />
          
          <main className="main-content">
            {!walletConnected ? (
              <ConnectWallet onConnect={handleWalletConnect} />
            ) : (
              <Routes>
                <Route path="/" element={<Dashboard walletAddress={walletAddress} />} />
                <Route path="/tokens" element={<TokenBalances walletAddress={walletAddress} />} />
                <Route path="/yield" element={<YieldPositions walletAddress={walletAddress} />} />
              </Routes>
            )}
          </main>
        </div>
      </Router>
    </Web3ReactProvider>
  );
}

export default App; 