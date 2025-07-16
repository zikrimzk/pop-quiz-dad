@extends('layouts.main')

@php

    use Carbon\Carbon;

@endphp
@section('content')
    <!-- Start::app-content -->
    <div class="main-content app-content">
        <div class="container-fluid">

            <!-- Page Header -->
            <div class="d-md-flex d-block align-items-center justify-content-between my-4 page-header-breadcrumb">
                <div>
                    <h1 class="page-title fw-semibold fs-18 mb-1">Welcome back,
                        {{ session('api_user.username') }} !</h1>
                </div>
                <div class="ms-md-1 ms-0">
                    <nav>
                        <ol class="breadcrumb mb-0">
                            <li class="breadcrumb-item active" aria-current="page">Create Quiz</li>
                        </ol>
                    </nav>
                </div>
            </div>
            <!-- Page Header Close -->

            <!-- Start::Crate Quiz Form -->
            <div class="row">
                <div class="col-sm-8">
                    @if (session('success'))
                        <div class="alert alert-success alert-dismissible fade show" role="alert">
                            {{ session('success') }}
                            <button class="btn-close" data-bs-dismiss="alert"></button>
                        </div>
                    @endif

                    @if ($errors->any())
                        <div class="alert alert-danger">
                            <ul class="ps-3 mb-0">
                                @foreach ($errors->all() as $e)
                                    <li>{{ $e }}</li>
                                @endforeach
                            </ul>
                        </div>
                    @endif
                    @if (session('error_list'))
                        <div class="alert alert-warning">
                            <ul class="ps-3 mb-0">
                                @foreach (session('error_list') as $e)
                                    <li>{{ $e }}</li>
                                @endforeach
                            </ul>
                        </div>
                    @elseif(session('error'))
                        <div class="alert alert-warning">{{ session('error') }}</div>
                    @endif
                    <form action="{{ route('create-quiz') }}" method="POST">
                        @csrf
                        <div class="card shadow-lg">
                            <div class="card-body">
                                <h5 class="card-title mb-4">Create Quizzes</h5>

                                <div class="row">
                                    <div class="col-sm-12">
                                        <div class="mb-3">
                                            <label for="quizTitle" class="form-label">Quiz Title</label>
                                            <input type="text" class="form-control" id="quizTitle"
                                                placeholder="Enter Quiz Title" name="quizTitle">
                                        </div>
                                        <div class="mb-3">
                                            <label for="quizDescription" class="form-label">Quiz Description</label>
                                            <textarea class="form-control" id="quizDescription" placeholder="Enter Quiz Description" name="quizDescription"
                                                rows="6"></textarea>
                                        </div>
                                    </div>
                                    <div class="col-sm-6">
                                        <div class="mb-3">
                                            <label for="quizStartTime" class="form-label">Start Time</label>
                                            <input type="datetime-local" class="form-control" id="quizStartTime"
                                                name="quizStartTime">
                                        </div>
                                    </div>
                                    <div class="col-sm-6">
                                        <div class="mb-3">
                                            <label for="quizEndTime" class="form-label">End Time</label>
                                            <input type="datetime-local" class="form-control" id="quizEndTime"
                                                name="quizEndTime">
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="card-footer border-0">
                                <div class="d-flex justify-content-between gap-3 py-2">
                                    <button type="reset" class="btn btn-secondary w-50">Reset</button>
                                    <button type="submit" class="btn btn-primary w-50">Create Quiz</button>
                                </div>
                            </div>
                        </div>
                    </form>

                </div>

                <div class="col-sm-4">
                    <div class="card shadow-lg">
                        <div class="card-body">
                            <h5 class="card-title mb-4">Created Quizzes</h5>

                            @if (count($quizzes))
                                <ul class="list-unstyled mb-0">
                                    @foreach ($quizzes as $quiz)
                                        <li class="mb-3 pb-2 border-bottom">
                                            <h6 class="mb-1">{{ $quiz['title'] }}</h6>
                                            <p class="mb-1 text-muted">{{ $quiz['description'] }}</p>
                                            <small class="d-block">
                                                <strong>Start:</strong>
                                                {{ Carbon::parse($quiz['startTime'])->format('d M Y, H:i') }}
                                            </small>
                                            <small class="d-block">
                                                <strong>End:</strong>
                                                {{ Carbon::parse($quiz['endTime'])->format('d M Y, H:i') }}
                                            </small>
                                            <small class="badge bg-{{ $quiz['status'] ? 'success' : 'secondary' }}">
                                                {{ $quiz['status'] ? 'Active' : 'Inactive' }}
                                            </small>
                                            <div class="mt-2">
                                                <a data-bs-toggle="modal" data-bs-target="#qrModal-{{ $quiz['uniqueId'] }}"
                                                    class="btn btn-sm btn-outline-danger">
                                                    View QR Code
                                                </a>
                                                <a href="{{ '/join-game/' . $quiz['uniqueId'] }}"
                                                    class="btn btn-sm btn-outline-primary">
                                                    Join URL
                                                </a>

                                                 <a href="{{ '/dashboard/' . $quiz['uniqueId'] }}"
                                                    class="btn btn-sm btn-outline-secondary">
                                                      <span class="ms-2 text-muted">Code:
                                                    <code>{{ $quiz['uniqueId'] }}</code></span>
                                                </a>
                                              
                                            </div>
                                        </li>

                                        <!-- Modal -->
                                        <div class="modal fade" id="qrModal-{{ $quiz['uniqueId'] }}" tabindex="-1" aria-hidden="true">
                                            <div class="modal-dialog      ">
                                                <div class="modal-content">
                                                    <div class="modal-header">
                                                        <h5 class="modal-title" id="qrModalLabel">QR Code</h5>
                                                        <button type="button" class="btn-close" data-bs-dismiss="modal"
                                                            aria-label="Close"></button>
                                                    </div>
                                                    <div class="modal-body text-center">
                                                        <img src="https://api.qrserver.com/v1/create-qr-code/?data={{ 'http://127.0.0.1/join-game/' . $quiz['uniqueId'] }}&amp;size=400x400"
                                                            alt="qrcode" title="{{ $quiz['uniqueId'] }}" class="img-fluid" />
                                                    </div>
                                                       <h5 class="modal-title" id="qrModalLabel">{{ $quiz['uniqueId']}}</h5>
                                                </div>
                                            </div>
                                        </div>
                                    @endforeach
                                </ul>
                            @else
                                <p class="text-muted mb-0">You haven’t created any quizzes yet.</p>
                            @endif

                        </div>
                    </div>
                </div>
            </div>
            <!--End::Crate Quiz Form -->



        </div>
    </div>
    <!-- End::app-content -->
@endsection
