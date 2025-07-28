#!/usr/bin/env node

/**
 * Simple Filesystem MCP Server for BusBuddy
 * Provides file system access via MCP protocol
 */

const fs = require('fs').promises;
const path = require('path');

class FilesystemMCPServer {
    constructor() {
        this.allowedDirectories = process.env.ALLOWED_DIRECTORIES?.split(';') || [process.cwd()];
    }

    async initialize() {
        console.log('Filesystem MCP Server initialized');
        console.log('Allowed directories:', this.allowedDirectories);
    }

    async listFiles(directory) {
        const fullPath = path.resolve(directory);
        
        // Security check
        if (!this.allowedDirectories.some(allowed => fullPath.startsWith(path.resolve(allowed)))) {
            throw new Error('Directory access denied');
        }

        try {
            const files = await fs.readdir(fullPath, { withFileTypes: true });
            return files.map(file => ({
                name: file.name,
                type: file.isDirectory() ? 'directory' : 'file',
                path: path.join(fullPath, file.name)
            }));
        } catch (error) {
            throw new Error(Failed to list files: ${error.message});
        }
    }

    async readFile(filePath) {
        const fullPath = path.resolve(filePath);
        
        // Security check
        if (!this.allowedDirectories.some(allowed => fullPath.startsWith(path.resolve(allowed)))) {
            throw new Error('File access denied');
        }

        try {
            const content = await fs.readFile(fullPath, 'utf8');
            return {
                path: fullPath,
                content: content,
                size: content.length
            };
        } catch (error) {
            throw new Error(Failed to read file: ${error.message});
        }
    }

    async writeFile(filePath, content) {
        const fullPath = path.resolve(filePath);
        
        // Security check
        if (!this.allowedDirectories.some(allowed => fullPath.startsWith(path.resolve(allowed)))) {
            throw new Error('File write access denied');
        }

        try {
            await fs.writeFile(fullPath, content, 'utf8');
            return {
                path: fullPath,
                success: true,
                size: content.length
            };
        } catch (error) {
            throw new Error(Failed to write file: ${error.message});
        }
    }
}

// Simple MCP protocol handler
class MCPHandler {
    constructor() {
        this.server = new FilesystemMCPServer();
    }

    async handleRequest(request) {
        try {
            await this.server.initialize();
            
            switch (request.method) {
                case 'filesystem/list':
                    return await this.server.listFiles(request.params.directory || '.');
                case 'filesystem/read':
                    return await this.server.readFile(request.params.path);
                case 'filesystem/write':
                    return await this.server.writeFile(request.params.path, request.params.content);
                default:
                    throw new Error(Unknown method: ${request.method});
            }
        } catch (error) {
            return { error: error.message };
        }
    }
}

// Start server
if (require.main === module) {
    const handler = new MCPHandler();
    console.log('BusBuddy Filesystem MCP Server starting...');
    console.log('Environment:', {
        cwd: process.cwd(),
        allowedDirs: process.env.ALLOWED_DIRECTORIES
    });
    
    // Simple test
    handler.handleRequest({
        method: 'filesystem/list',
        params: { directory: '.' }
    }).then(result => {
        console.log('Test result:', result);
        console.log('Server ready for MCP connections');
    }).catch(error => {
        console.error('Server startup error:', error);
    });
}

module.exports = { FilesystemMCPServer, MCPHandler };
