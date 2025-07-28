#!/usr/bin/env node

/**
 * Simple Git MCP Server for BusBuddy
 * Provides Git operations via MCP protocol
 */

const { execSync } = require('child_process');
const path = require('path');

class GitMCPServer {
    constructor() {
        this.allowedRepositories = process.env.ALLOWED_REPOSITORIES?.split(';') || [process.cwd()];
    }

    async initialize() {
        console.log('Git MCP Server initialized');
        console.log('Allowed repositories:', this.allowedRepositories);
    }

    async executeGitCommand(command, repository = '.') {
        const fullPath = path.resolve(repository);

        // Security check
        if (!this.allowedRepositories.some(allowed => fullPath.startsWith(path.resolve(allowed)))) {
            throw new Error('Repository access denied');
        }

        try {
            const result = execSync(command, {
                cwd: fullPath,
                encoding: 'utf8',
                maxBuffer: 1024 * 1024 // 1MB buffer
            });
            return {
                command: command,
                output: result,
                repository: fullPath,
                success: true
            };
        } catch (error) {
            throw new Error(`Git command failed: ${error.message}`);
        }
    }

    async getStatus(repository = '.') {
        return await this.executeGitCommand('git status --porcelain', repository);
    }

    async getLog(repository = '.', limit = 10) {
        return await this.executeGitCommand(`git log --oneline -n ${limit}`, repository);
    }

    async getBranches(repository = '.') {
        return await this.executeGitCommand('git branch -a', repository);
    }

    async getDiff(repository = '.', file = '') {
        const command = file ? `git diff ${file}` : 'git diff';
        return await this.executeGitCommand(command, repository);
    }
}

// Simple MCP protocol handler
class GitMCPHandler {
    constructor() {
        this.server = new GitMCPServer();
    }

    async handleRequest(request) {
        try {
            await this.server.initialize();

            switch (request.method) {
                case 'git/status':
                    return await this.server.getStatus(request.params.repository);
                case 'git/log':
                    return await this.server.getLog(request.params.repository, request.params.limit);
                case 'git/branches':
                    return await this.server.getBranches(request.params.repository);
                case 'git/diff':
                    return await this.server.getDiff(request.params.repository, request.params.file);
                case 'git/command':
                    return await this.server.executeGitCommand(request.params.command, request.params.repository);
                default:
                    throw new Error(`Unknown method: ${request.method}`);
            }
        } catch (error) {
            return { error: error.message };
        }
    }
}

// Start server
if (require.main === module) {
    const handler = new GitMCPHandler();
    console.log('BusBuddy Git MCP Server starting...');
    console.log('Environment:', {
        cwd: process.cwd(),
        allowedRepos: process.env.ALLOWED_REPOSITORIES
    });

    // Simple test
    handler.handleRequest({
        method: 'git/status',
        params: { repository: '.' }
    }).then(result => {
        console.log('Test result:', result);
        console.log('Server ready for MCP connections');
    }).catch(error => {
        console.error('Server startup error:', error);
    });
}

module.exports = { GitMCPServer, GitMCPHandler };
